// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings.ExpandLinqMethodOperation
{
    internal abstract class ExtractLinqToLocalFunctionRefactoring
    {
        private TypeSyntax _elementType;

        protected ExtractLinqToLocalFunctionRefactoring(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            SyntaxNode body,
            SyntaxNode containingBody,
            ITypeSymbol elementTypeSymbol,
            ImmutableArray<ISymbol> capturedSymbols,
            SemanticModel semanticModel)
        {
            Document = document;
            InvocationExpression = invocationExpression;
            Body = body;
            ContainingBody = containingBody;
            ElementTypeSymbol = elementTypeSymbol;
            CapturedSymbols = capturedSymbols;
            SemanticModel = semanticModel;
        }

        public Document Document { get; }

        public InvocationExpressionSyntax InvocationExpression { get; private set; }

        public SyntaxNode Body { get; }

        public SyntaxNode ContainingBody { get; }

        public ITypeSymbol ElementTypeSymbol { get; }

        public ImmutableArray<ISymbol> CapturedSymbols { get; }

        public TypeSyntax ElementType
        {
            get
            {
                if (_elementType == null)
                {
                    if (ElementTypeSymbol.SupportsExplicitDeclaration())
                    {
                        _elementType = ElementTypeSymbol.ToTypeSyntax().WithSimplifierAnnotation();
                    }
                    else
                    {
                        _elementType = VarType();
                    }
                }

                return _elementType;
            }
        }

        public ParameterSyntax Parameter
        {
            get
            {
                switch (Body.Parent.Kind())
                {
                    case SyntaxKind.SimpleLambdaExpression:
                        return ((SimpleLambdaExpressionSyntax)Body.Parent).Parameter;
                    case SyntaxKind.ParenthesizedLambdaExpression:
                        return ((ParenthesizedLambdaExpressionSyntax)Body.Parent).ParameterList?.Parameters.FirstOrDefault();
                    case SyntaxKind.AnonymousMethodExpression:
                        return ((AnonymousMethodExpressionSyntax)Body.Parent).ParameterList?.Parameters.FirstOrDefault();
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public SemanticModel SemanticModel { get; }

        public abstract string MethodName { get; }

        protected abstract ReturnStatementSyntax GetFirstReturnStatement();

        protected abstract ReturnStatementSyntax GetLastReturnStatement();

        protected abstract TypeSyntax GetReturnType();

        protected virtual ExpressionSyntax GetCondition(ExpressionSyntax expression)
        {
            return expression;
        }

        public Task<Document> RefactorAsync(CancellationToken cancellationToken)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(InvocationExpression);

            ExpressionSyntax argumentExpression = invocationInfo
                .Arguments
                .Single()
                .Expression
                .WalkDownParentheses();

            AnonymousFunctionAnalysis analysis = AnonymousFunctionAnalysis.Create(argumentExpression, SemanticModel);

            TypeSyntax elementType;
            if (ElementTypeSymbol.SupportsExplicitDeclaration())
            {
                elementType = ElementTypeSymbol.ToTypeSyntax().WithSimplifierAnnotation();
            }
            else
            {
                elementType = VarType();
            }

            string parameterName = Parameter.Identifier.ValueText;

            string functionName = MethodName;

            LocalFunctionStatementSyntax childLocalFunction = null;

            SyntaxNode declaration = null;

            int position;

            if (ContainingBody is BlockSyntax body)
            {
                position = body.Statements.Last().Span.End;
            }
            else
            {
                var expressionBody = (ArrowExpressionClauseSyntax)ContainingBody;

                position = expressionBody.Expression.Span.End;

                int offset = InvocationExpression.FullSpan.Start - expressionBody.Expression.FullSpan.Start;

                (SyntaxNode node, BlockSyntax body) result = ConvertExpressionBodyToBlockBodyRefactoring.Refactor(expressionBody, SemanticModel, cancellationToken);

                declaration = result.node;
                body = result.body;

                var returnStatement = ((ReturnStatementSyntax)body.Statements.First());

                SyntaxToken returnKeyword = returnStatement.ReturnKeyword;

                InvocationExpression = (InvocationExpressionSyntax)returnStatement.FindNode(
                    new TextSpan(offset + returnKeyword.Span.Length + returnStatement.FullSpan.Start, InvocationExpression.FullSpan.Length),
                    getInnermostNodeForTie: true);
            }

            ExpressionSyntax condition;

            if (analysis.Body is BlockSyntax block)
            {
                functionName = NameGenerator.Default.EnsureUniqueLocalName(functionName, SemanticModel, ContainingBody.SpanStart, cancellationToken: cancellationToken);

                condition = InvocationExpression(
                    IdentifierName(functionName),
                    ArgumentList(Argument(IdentifierName(parameterName))));

                childLocalFunction = LocalFunctionStatement(
                    default,
                    GetReturnType(),
                    Identifier(functionName),
                    ParameterList(Parameter(elementType, parameterName)),
                    block);

                childLocalFunction = childLocalFunction.WithFormatterAnnotation();

                functionName += "2";
            }
            else
            {
                condition = (ExpressionSyntax)analysis.Body;
            }

            ForEachStatementSyntax forEachStatement = ForEachStatement(
                elementType,
                Identifier(parameterName),
                invocationInfo.Expression,
                Block(
                    IfStatement(
                        GetCondition(condition),
                        Block(GetFirstReturnStatement()))));

            forEachStatement = forEachStatement.WithFormatterAnnotation();

            functionName = NameGenerator.Default.EnsureUniqueLocalName(functionName, SemanticModel, position, cancellationToken: cancellationToken);

            (SeparatedSyntaxList<ParameterSyntax> parameters, SeparatedSyntaxList<ArgumentSyntax> arguments)
                = GetArgumentsAndParameters(analysis, SemanticModel, position);

            LocalFunctionStatementSyntax newLocalFunction = LocalFunctionStatement(
                default,
                GetReturnType(),
                Identifier(functionName).WithRenameAnnotation(),
                ParameterList(parameters),
                (childLocalFunction != null)
                    ? Block(forEachStatement, GetLastReturnStatement(), childLocalFunction)
                    : Block(forEachStatement, GetLastReturnStatement()));

            newLocalFunction = newLocalFunction.WithFormatterAnnotation();

            BlockSyntax newBody = body
                .ReplaceNode(InvocationExpression, InvocationExpression(IdentifierName(functionName), ArgumentList(arguments)))
                .AddStatements(newLocalFunction);

            if (declaration != null)
            {
                newBody = newBody.WithFormatterAnnotation();

                SyntaxNode newDeclaration = declaration.ReplaceNode(body, newBody);

                return Document.ReplaceNodeAsync(ContainingBody.Parent, newDeclaration, cancellationToken);
            }
            else
            {
                return Document.ReplaceNodeAsync(body, newBody, cancellationToken);
            }
        }

        private static (SeparatedSyntaxList<ParameterSyntax> parameters, SeparatedSyntaxList<ArgumentSyntax> arguments) GetArgumentsAndParameters(
            in AnonymousFunctionAnalysis analysis,
            SemanticModel semanticModel,
            int position)
        {
            if (analysis.Captured.IsDefault)
                return default;

            SeparatedSyntaxList<ParameterSyntax> parameters = default;
            SeparatedSyntaxList<ArgumentSyntax> arguments = default;

            foreach (ISymbol symbol in analysis.Captured)
            {
                if (symbol is ILocalSymbol localSymbol)
                {
                    IdentifierNameSyntax identifierName = IdentifierName(symbol.Name);

                    if (semanticModel
                        .GetSpeculativeSymbolInfo(position, identifierName, SpeculativeBindingOption.BindAsExpression)
                        .Symbol?
                        .Equals(symbol) != true)
                    {
                        parameters = parameters.Add(Parameter(localSymbol.Type.ToTypeSyntax().WithSimplifierAnnotation(), localSymbol.Name));
                        arguments = arguments.Add(Argument(identifierName));
                    }
                }
                else if (symbol is IParameterSymbol parameterSymbol)
                {
                    if (!semanticModel.IsAccessible(position, parameterSymbol))
                    {
                        parameters = parameters.Add(Parameter(parameterSymbol.Type.ToTypeSyntax().WithSimplifierAnnotation(), parameterSymbol.Name));
                        arguments = arguments.Add(Argument(IdentifierName(parameterSymbol.Name)));
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return (parameters, arguments);
        }
    }
}
