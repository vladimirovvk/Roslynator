// Copyright (c) Josef Pihrt and Contributors. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseElementAccessRefactoring
    {
        public static void ComputeRefactorings(
            RefactoringContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            SemanticModel semanticModel)
        {
            InvocationExpressionSyntax invocation = invocationInfo.InvocationExpression;

            if (invocation.IsParentKind(SyntaxKind.ExpressionStatement))
                return;

            switch (invocationInfo.NameText)
            {
                case "First":
                    {
                        if (invocationInfo.Arguments.Any())
                            break;

                        if (!UseElementAccessAnalysis.IsFixableFirst(invocationInfo, semanticModel, context.CancellationToken))
                            break;

                        context.RegisterRefactoring(
                            "Use [] instead of calling 'First'",
                            ct => UseElementAccessInsteadOfEnumerableMethodRefactoring.UseElementAccessInsteadOfFirstAsync(context.Document, invocation, ct),
                            RefactoringDescriptors.UseElementAccessInsteadOfLinqMethod);

                        break;
                    }
                case "Last":
                    {
                        if (invocationInfo.Arguments.Any())
                            break;

                        if (!UseElementAccessAnalysis.IsFixableLast(invocationInfo, semanticModel, context.CancellationToken))
                            break;

                        string propertyName = CSharpUtility.GetCountOrLengthPropertyName(invocationInfo.Expression, semanticModel, context.CancellationToken);

                        if (propertyName == null)
                            break;

                        context.RegisterRefactoring(
                            "Use [] instead of calling 'Last'",
                            ct => UseElementAccessInsteadOfEnumerableMethodRefactoring.UseElementAccessInsteadOfLastAsync(context.Document, invocation, propertyName, ct),
                            RefactoringDescriptors.UseElementAccessInsteadOfLinqMethod);

                        break;
                    }
                case "ElementAt":
                    {
                        if (invocationInfo.Arguments.SingleOrDefault(shouldThrow: false)?.Expression?.IsMissing != false)
                            break;

                        if (!UseElementAccessAnalysis.IsFixableElementAt(invocationInfo, semanticModel, context.CancellationToken))
                            break;

                        context.RegisterRefactoring(
                            "Use [] instead of calling 'ElementAt'",
                            ct => UseElementAccessInsteadOfEnumerableMethodRefactoring.UseElementAccessInsteadOfElementAtAsync(context.Document, invocation, ct),
                            RefactoringDescriptors.UseElementAccessInsteadOfLinqMethod);

                        break;
                    }
            }
        }
    }
}
