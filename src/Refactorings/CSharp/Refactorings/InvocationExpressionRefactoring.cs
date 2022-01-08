// Copyright (c) Josef Pihrt and Contributors. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings.ExpandLinqMethodOperation;
using Roslynator.CSharp.Refactorings.InlineDefinition;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class InvocationExpressionRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, InvocationExpressionSyntax invocationExpression)
        {
            if (context.IsAnyRefactoringEnabled(
                RefactoringDescriptors.UseElementAccessInsteadOfLinqMethod,
                RefactoringDescriptors.InvertLinqMethodCall,
                RefactoringDescriptors.CallExtensionMethodAsInstanceMethod,
                RefactoringDescriptors.CallIndexOfInsteadOfContains,
                RefactoringDescriptors.ExpandLinqMethodOperation))
            {
                SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

                if (invocationInfo.Success)
                {
                    if (context.Span.IsEmptyAndContainedInSpan(invocationInfo.Name))
                    {
                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        if (context.IsRefactoringEnabled(RefactoringDescriptors.UseElementAccessInsteadOfLinqMethod))
                            UseElementAccessRefactoring.ComputeRefactorings(context, invocationInfo, semanticModel);

                        if (context.IsRefactoringEnabled(RefactoringDescriptors.ExpandLinqMethodOperation))
                            ExpandLinqMethodOperationRefactoring.ComputeRefactorings(context, invocationInfo, semanticModel);

                        if (context.IsRefactoringEnabled(RefactoringDescriptors.InvertLinqMethodCall))
                            InvertLinqMethodCallRefactoring.ComputeRefactoring(context, invocationExpression, semanticModel);

                        if (context.IsRefactoringEnabled(RefactoringDescriptors.CallIndexOfInsteadOfContains))
                            CallIndexOfInsteadOfContainsRefactoring.ComputeRefactoring(context, invocationExpression, semanticModel);
                    }

                    if (context.IsRefactoringEnabled(RefactoringDescriptors.CallExtensionMethodAsInstanceMethod))
                    {
                        SyntaxNodeOrToken nodeOrToken = CallExtensionMethodAsInstanceMethodAnalysis.GetNodeOrToken(invocationExpression.Expression);

                        if (nodeOrToken.Span.Contains(context.Span))
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            CallExtensionMethodAsInstanceMethodAnalysisResult analysis = CallExtensionMethodAsInstanceMethodAnalysis.Analyze(invocationExpression, semanticModel, allowAnyExpression: true, cancellationToken: context.CancellationToken);

                            if (analysis.Success)
                            {
                                context.RegisterRefactoring(
                                    CallExtensionMethodAsInstanceMethodRefactoring.Title,
                                    ct =>
                                    {
                                        return context.Document.ReplaceNodeAsync(
                                            analysis.InvocationExpression,
                                            analysis.NewInvocationExpression,
                                            ct);
                                    },
                                    RefactoringDescriptors.CallExtensionMethodAsInstanceMethod);
                            }
                        }
                    }
                }
            }

            if (context.IsRefactoringEnabled(RefactoringDescriptors.ConvertStringFormatToInterpolatedString)
                && context.SupportsCSharp6)
            {
                await ConvertStringFormatToInterpolatedStringRefactoring.ComputeRefactoringsAsync(context, invocationExpression).ConfigureAwait(false);
            }

            if (context.IsRefactoringEnabled(RefactoringDescriptors.ConvertHasFlagCallToBitwiseOperation))
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                if (ConvertHasFlagCallToBitwiseOperationAnalysis.IsFixable(invocationExpression, semanticModel, context.CancellationToken))
                {
                    context.RegisterRefactoring(
                        ConvertHasFlagCallToBitwiseOperationRefactoring.Title,
                        ct =>
                        {
                            return ConvertHasFlagCallToBitwiseOperationRefactoring.RefactorAsync(
                                context.Document,
                                invocationExpression,
                                semanticModel,
                                ct);
                        },
                        RefactoringDescriptors.ConvertHasFlagCallToBitwiseOperation);
                }
            }

            if (context.IsRefactoringEnabled(RefactoringDescriptors.InlineMethod))
                await InlineMethodRefactoring.ComputeRefactoringsAsync(context, invocationExpression).ConfigureAwait(false);
        }
    }
}
