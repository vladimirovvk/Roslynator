#region usings
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
#endregion usings

namespace Sample
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SampleCodeFixProvider))]
    [Shared]
    public sealed class SampleCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(SampleAnalyzer.Id); }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            CompilationUnitSyntax compilationUnit = root.FirstAncestorOrSelf<CompilationUnitSyntax>();

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];
            string banner = diagnostic.Properties["banner"];

            CodeAction codeAction = CodeAction.Create(
                "Add header",
                ct => ApplyCodeFix(document, compilationUnit, banner, ct),
                diagnostic.Id);

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> ApplyCodeFix(
            Document document,
            CompilationUnitSyntax compilationUnit,
            string banner,
            CancellationToken cancellationToken)
        {
            CompilationUnitSyntax newCompilationUnit = GetNewRoot(compilationUnit, banner);

            return Task.FromResult(document.WithSyntaxRoot(newCompilationUnit));
        }

        public static CompilationUnitSyntax GetNewRoot(
            CompilationUnitSyntax compilationUnit,
            string banner)
        {
            SyntaxTriviaList newTrivia = SyntaxFactory.ParseLeadingTrivia($"// {banner.TrimStart()}" + Environment.NewLine);

            SyntaxToken token = compilationUnit.GetFirstToken();

            SyntaxTrivia comment = token.LeadingTrivia.FirstOrDefault(f => f.IsKind(SyntaxKind.SingleLineCommentTrivia));

            SyntaxToken newToken = token;
            if (comment.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                SyntaxTriviaList newLeadingTrivia = token.LeadingTrivia.Skip(2).ToSyntaxTriviaList().InsertRange(0, newTrivia);

                newToken = newToken.WithLeadingTrivia(newLeadingTrivia);
            }
            else
            {
                newToken = token.WithLeadingTrivia(token.LeadingTrivia.InsertRange(0, newTrivia));
            }

            return compilationUnit.ReplaceToken(token, newToken);
        }
    }
}
