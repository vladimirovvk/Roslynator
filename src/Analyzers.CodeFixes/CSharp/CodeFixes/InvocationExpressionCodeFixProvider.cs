// Copyright (c) Josef Pihrt and Contributors. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InvocationExpressionCodeFixProvider))]
    [Shared]
    public sealed class InvocationExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod,
                    DiagnosticIdentifiers.RemoveRedundantToStringCall,
                    DiagnosticIdentifiers.RemoveRedundantStringToCharArrayCall,
                    DiagnosticIdentifiers.CombineEnumerableWhereMethodChain,
                    DiagnosticIdentifiers.CallExtensionMethodAsInstanceMethod,
                    DiagnosticIdentifiers.CallThenByInsteadOfOrderBy,
                    DiagnosticIdentifiers.UseForEachInsteadOfForEachMethod);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out InvocationExpressionSyntax invocation))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.CombineEnumerableWhereMethodChain:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Combine 'Where' method chain",
                                ct => CombineEnumerableWhereMethodChainRefactoring.RefactorAsync(context.Document, invocation, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod:
                        {
                            string propertyName = diagnostic.Properties["PropertyName"];

                            CodeAction codeAction = CodeAction.Create(
                                $"Use '{propertyName}' property instead of calling 'Any'",
                                ct => UseCountOrLengthPropertyInsteadOfAnyMethodRefactoring.RefactorAsync(context.Document, invocation, propertyName, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantToStringCall:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant 'ToString' call",
                                ct => context.Document.ReplaceNodeAsync(invocation, RemoveInvocation(invocation).WithFormatterAnnotation(), ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantStringToCharArrayCall:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant 'ToCharArray' call",
                                ct => context.Document.ReplaceNodeAsync(invocation, RemoveInvocation(invocation).WithFormatterAnnotation(), ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.CallExtensionMethodAsInstanceMethod:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                CallExtensionMethodAsInstanceMethodRefactoring.Title,
                                ct => CallExtensionMethodAsInstanceMethodRefactoring.RefactorAsync(context.Document, invocation, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.CallThenByInsteadOfOrderBy:
                        {
                            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocation);

                            string oldName = invocationInfo.NameText;

                            string newName = (string.Equals(oldName, "OrderBy", StringComparison.Ordinal))
                                ? "ThenBy"
                                : "ThenByDescending";

                            CodeAction codeAction = CodeAction.Create(
                                $"Call '{newName}' instead of '{oldName}'",
                                ct => CallThenByInsteadOfOrderByRefactoring.RefactorAsync(context.Document, invocation, newName, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseForEachInsteadOfForEachMethod:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Convert to 'foreach'",
                                ct => ConvertForEachMethodToForEachAsync(context.Document, invocation, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static async Task<Document> ConvertForEachMethodToForEachAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ISymbol symbol = semanticModel.GetSymbol(invocationExpression, cancellationToken);

            ExpressionSyntax collectionExpression = null;
            ExpressionSyntax anonymousMethodExpression = null;

            if (symbol.ContainingType.SpecialType == SpecialType.System_Array)
            {
                collectionExpression = invocationInfo.Arguments[0]
                    .Expression
                    .WalkDownParentheses();

                anonymousMethodExpression = invocationInfo.Arguments[1]
                    .Expression
                    .WalkDownParentheses();
            }
            else if (symbol.ContainingType.OriginalDefinition.HasMetadataName(MetadataNames.System_Collections_Generic_List_T))
            {
                collectionExpression = invocationInfo.Expression;

                anonymousMethodExpression = invocationInfo.Arguments[0]
                    .Expression
                    .WalkDownParentheses();
            }
            else
            {
                throw new InvalidOperationException();
            }

            SyntaxToken identifier = default;
            BlockSyntax block = null;

            switch (anonymousMethodExpression.Kind())
            {
                case SyntaxKind.SimpleLambdaExpression:
                    {
                        var lambda = (SimpleLambdaExpressionSyntax)anonymousMethodExpression;

                        identifier = lambda.Parameter.Identifier;

                        block = lambda.Body as BlockSyntax ?? Block(ExpressionStatement((ExpressionSyntax)lambda.Body));
                        break;
                    }
                case SyntaxKind.ParenthesizedLambdaExpression:
                    {
                        var lambda = (ParenthesizedLambdaExpressionSyntax)anonymousMethodExpression;

                        identifier = lambda.ParameterList.Parameters.Single().Identifier;

                        block = lambda.Body as BlockSyntax ?? Block(ExpressionStatement((ExpressionSyntax)lambda.Body));
                        break;
                    }
                case SyntaxKind.AnonymousMethodExpression:
                    {
                        var anonymousMethod = (AnonymousMethodExpressionSyntax)anonymousMethodExpression;

                        identifier = anonymousMethod.ParameterList.Parameters.Single().Identifier;

                        block = anonymousMethod.Block;
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }

            var expressionStatement = (ExpressionStatementSyntax)invocationExpression.Parent;

            ForEachStatementSyntax forEachStatement = ForEachStatement(VarType(), identifier, collectionExpression, block)
                .WithTriviaFrom(expressionStatement)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(expressionStatement, forEachStatement, cancellationToken).ConfigureAwait(false);
        }

        private static ExpressionSyntax RemoveInvocation(InvocationExpressionSyntax invocation)
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            ArgumentListSyntax argumentList = invocation.ArgumentList;

            SyntaxToken closeParen = argumentList.CloseParenToken;

            return memberAccess.Expression
                .AppendToTrailingTrivia(
                    memberAccess.OperatorToken.GetAllTrivia()
                        .Concat(memberAccess.Name.GetLeadingAndTrailingTrivia())
                        .Concat(argumentList.OpenParenToken.GetAllTrivia())
                        .Concat(closeParen.LeadingTrivia)
                        .ToSyntaxTriviaList()
                        .EmptyIfWhitespace()
                        .AddRange(closeParen.TrailingTrivia));
        }
    }
}
