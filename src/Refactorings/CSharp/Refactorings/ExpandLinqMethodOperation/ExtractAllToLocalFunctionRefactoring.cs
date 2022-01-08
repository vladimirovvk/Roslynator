// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings.ExpandLinqMethodOperation
{
    internal sealed class ExtractAllToLocalFunctionRefactoring : ExtractLinqToLocalFunctionRefactoring
    {
        public ExtractAllToLocalFunctionRefactoring(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            SyntaxNode body,
            SyntaxNode containingBody,
            ITypeSymbol elementTypeSymbol,
            ImmutableArray<ISymbol> capturedSymbols,
            SemanticModel semanticModel) : base(document, invocationExpression, body, containingBody, elementTypeSymbol, capturedSymbols, semanticModel)
        {
        }

        public override string MethodName
        {
            get { return "All"; }
        }

        protected override ExpressionSyntax GetCondition(ExpressionSyntax expression)
        {
            return SyntaxLogicalInverter.GetInstance(Document).LogicallyInvert(expression, SemanticModel);
        }

        protected override ReturnStatementSyntax GetFirstReturnStatement()
        {
            return ReturnStatement(FalseLiteralExpression());
        }

        protected override ReturnStatementSyntax GetLastReturnStatement()
        {
            return ReturnStatement(TrueLiteralExpression());
        }

        protected override TypeSyntax GetReturnType()
        {
            return CSharpTypeFactory.BoolType();
        }
    }
}
