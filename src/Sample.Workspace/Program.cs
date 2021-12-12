#region usings
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
#endregion usings

#pragma warning disable RCS1090

namespace Sample
{
    internal static class WorkspaceApp
    {
        internal static async Task Main()
        {
            CancellationToken cancellationToken = CancellationToken.None;

            MSBuildLocator.RegisterDefaults();

            using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            Solution solution = await workspace.OpenSolutionAsync(@"C:\code\public\roslynator\src\sample.code.sln");

            foreach (Project project in solution.Projects
                .OrderBy(f => f.Name))
            {
                Console.WriteLine(project.Name);

                foreach (Document document in project.Documents.OrderBy(f => f.Name))
                {
                    Console.WriteLine("  " + document.Name);

                    SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);

                    if (!project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(root.SyntaxTree).TryGetValue(SampleAnalyzer.OptionKey, out string banner))
                        continue;

                    CompilationUnitSyntax newDocument = SampleCodeFixProvider.GetNewRoot((CompilationUnitSyntax)root, banner);

                    solution = solution.WithDocumentSyntaxRoot(document.Id, newDocument);
                }
            }

            workspace.TryApplyChanges(solution);

            foreach (Project project in solution.Projects
                .OrderBy(f => f.Name))
            {
                Console.WriteLine(project.Name);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                var analyzer = new SampleAnalyzer();

                var options = new CompilationWithAnalyzersOptions(
                    project.AnalyzerOptions,
                    null,
                    true,
                    false);

                var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation!, ImmutableArray.Create<DiagnosticAnalyzer>(analyzer), options);

                ImmutableArray<Diagnostic> diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

                var codeFixProvider = new SampleCodeFixProvider();

                foreach (IGrouping<SyntaxTree, Diagnostic> grouping in diagnostics
                    .GroupBy(f => f.Location.SourceTree))
                {
                    Document document = project.GetDocument(grouping.Key)!;

                    foreach (Diagnostic diagnostic in grouping)
                    {
                        CodeAction fix = null;

                        var context = new CodeFixContext(
                            solution.GetDocument(document.Id)!,
                            diagnostic,
                            (a, _) => fix = a,
                            cancellationToken);

                        await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

                        if (fix != null)
                        {
                            ImmutableArray<CodeActionOperation> operations = await fix.GetOperationsAsync(cancellationToken);

                            CodeActionOperation operation = operations.Single();

                            operations[0].Apply(workspace, cancellationToken);

                            solution = workspace.CurrentSolution;
                        }
                    }
                }
            }
        }
    }
}
