#region usings
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
#endregion usings

#pragma warning disable RCS1090

namespace Sample
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SampleCodeRefactoringProvider))]
    public sealed class SampleCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            Document document = context.Document;
            TextSpan span = context.Span;

            SyntaxNode root = await document.GetSyntaxRootAsync(context.CancellationToken);

            LocalDeclarationStatementSyntax localDeclaration = root.FindNode(span).FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();

            if (localDeclaration != null)
            {
                TypeSyntax type = localDeclaration.Declaration.Type;
                if (type.IsVar
                    && type.Span.Contains(span))
                {
                    CodeAction codeAction = CodeAction.Create(
                        "Convert to explicit type",
                        ct => ApplyRefactoringAsync(document, localDeclaration, ct),
                        equivalenceKey: "sample.convert_to_explicit_type");

                    context.RegisterRefactoring(codeAction);
                }

                await Task.CompletedTask;
            }
        }

        private static async Task<Document> ApplyRefactoringAsync(
            Document document,
            LocalDeclarationStatementSyntax localDeclaration,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            VariableDeclarationSyntax variableDeclaration = localDeclaration.Declaration;

            VariableDeclaratorSyntax variableDeclarator = variableDeclaration.Variables.First();

            ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(variableDeclarator.Initializer.Value, cancellationToken).Type;

            string typeText = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            TypeSyntax newType = SyntaxFactory.ParseTypeName(typeText).WithAdditionalAnnotations(Simplifier.Annotation);

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);

            SyntaxNode newRoot = root.ReplaceNode(variableDeclaration.Type, newType);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
