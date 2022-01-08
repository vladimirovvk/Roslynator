// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.Analysis
{
    internal static class UseForEachInsteadOfForEachMethodAnalysis
    {
        public static void AnalyzeList(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            if (!invocationExpression.IsParentKind(SyntaxKind.ExpressionStatement))
                return;

            ExpressionSyntax expression = invocationInfo
                .Arguments
                .SingleOrDefault(shouldThrow: false)?
                .Expression
                .WalkDownParentheses();

            if (expression == null)
                return;

            if (!CSharpFacts.IsAnonymousFunctionExpression(expression.Kind()))
                return;

            if (!(context.SemanticModel.GetSymbol(invocationExpression, context.CancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                return;

            if (methodSymbol.IsStatic)
                return;

            if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Void)
                return;

            if (methodSymbol.IsGenericMethod)
                return;

            methodSymbol = methodSymbol.OriginalDefinition;

            if (!methodSymbol.ContainingType.HasMetadataName(MetadataNames.System_Collections_Generic_List_T))
                return;

            IParameterSymbol parameter = methodSymbol.Parameters.SingleOrDefault(shouldThrow: false);

            if (parameter == null)
                return;

            if (!parameter.Type.HasMetadataName(MetadataNames.System_Action_1))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseForEachInsteadOfForEachMethod, invocationExpression);
        }

        public static void AnalyzeArray(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            if (!invocationExpression.IsParentKind(SyntaxKind.ExpressionStatement))
                return;

            ExpressionSyntax expression = invocationInfo
                .Arguments
                .Last()?
                .Expression
                .WalkDownParentheses();

            if (expression == null)
                return;

            if (!CSharpFacts.IsAnonymousFunctionExpression(expression.Kind()))
                return;

            if (!(context.SemanticModel.GetSymbol(invocationExpression, context.CancellationToken) is IMethodSymbol methodSymbol))
                return;

            if (methodSymbol.ContainingType.SpecialType != SpecialType.System_Array)
                return;

            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
                return;

            if (!methodSymbol.IsStatic)
                return;

            if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Void)
                return;

            if (methodSymbol.Arity != 1)
                return;

            ImmutableArray<IParameterSymbol> parameters = methodSymbol.OriginalDefinition.Parameters;

            if (parameters.Length != 2)
                return;

            if (parameters[0].Type.TypeKind != TypeKind.Array)
                return;

            if (!parameters[1].Type.HasMetadataName(MetadataNames.System_Action_1))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseForEachInsteadOfForEachMethod, invocationExpression);
        }
    }
}
