using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RS2008

namespace Sample
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SampleAnalyzer : DiagnosticAnalyzer
    {
        public const string OptionKey = "sample.file_banner";
        public const string Id = "SMP0001";

        public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: Id,
            title: "Add banner at the top of the file",
            messageFormat: "Add banner at the top of the file",
            category: "sample",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: null,
            helpLinkUri: null,
            customTags: Array.Empty<string>());

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Descriptor); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (((CSharpCompilation)startContext.Compilation).LanguageVersion >= LanguageVersion.CSharp5)
                {
                    startContext.RegisterSyntaxNodeAction(f => Analyze(f), SyntaxKind.CompilationUnit);
                }
            });
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            SyntaxNode node = context.Node;

            if (!context.Options.AnalyzerConfigOptionsProvider.GetOptions(node.SyntaxTree).TryGetValue(OptionKey, out string banner))
                return;

            SyntaxToken token = node.GetFirstToken();

            SyntaxTrivia trivia = token.LeadingTrivia.FirstOrDefault();

            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                string currentBanner = trivia.ToString().Substring(2).TrimStart();

                if (banner != currentBanner)
                {
                    ReportDiagnostic(context, token, banner);
                }
            }
            else
            {
                ReportDiagnostic(context, token, banner);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxToken token, string banner)
        {
            Diagnostic diagnostic = Diagnostic.Create(
                Descriptor,
                Location.Create(context.Node.SyntaxTree, new TextSpan(token.FullSpan.Start, 0)),
                new[] { new KeyValuePair<string, string>("banner", banner)}.ToImmutableDictionary());

            context.ReportDiagnostic(diagnostic);
        }
    }
}
