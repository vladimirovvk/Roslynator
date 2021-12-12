#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
#endregion usings

#pragma warning disable RCS1090

namespace Sample
{
    internal static class SyntaxApp
    {
        internal static async Task Main()
        {
            const string source = @"
using System;

class SampleClass
{
    /// <summary>
    /// 
    /// </summary>
    string SampleMethod()
    {
        var sampleLocal = new string('a', 3);
        return sampleLocal;
    }   
}
";
            using (Workspace workspace = new AdhocWorkspace())
            {
                IEnumerable<PortableExecutableReference> metadataReferences = AppContext
                    .GetData("TRUSTED_PLATFORM_ASSEMBLIES")
                    .ToString()
                    .Split(';')
                    .Select(f => MetadataReference.CreateFromFile(f));

                Solution solution = workspace.CurrentSolution;

                Document document = solution
                    .AddProject("Test", "Test", LanguageNames.CSharp)
                    .WithMetadataReferences(metadataReferences)
                    .AddDocument("Document", SourceText.From(source));

                SemanticModel semanticModel = await document.GetSemanticModelAsync();
                SyntaxTree syntaxTree = await document.GetSyntaxTreeAsync();
                SyntaxNode root = await syntaxTree.GetRootAsync();

                Console.WriteLine(root.ToFullString());
                Debug.Assert(root.ToFullString() == source);
                Console.WriteLine();

                var walker = new SampleSyntaxWalker(semanticModel);
                walker.Visit(root);
                Console.WriteLine();

                var rewriter = new SampleSyntaxRewriter();
                root = rewriter.Visit(root);
                Console.WriteLine(root.ToFullString());

                CSharpCompilation compilation = CSharpCompilation.Create(
                    Path.GetRandomFileName(),
                    syntaxTrees: new SyntaxTree[] { syntaxTree },
                    references: metadataReferences,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using var memoryStream = new MemoryStream();

                EmitResult emitResult = compilation.Emit(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);

                Assembly assembly = Assembly.Load(memoryStream.ToArray());

                Type[] types = assembly.GetTypes();

                Type type = types.First(f => f.Name == "SampleClass");

                object typeInstance = Activator.CreateInstance(type);

                var func = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), typeInstance, "SampleMethod");

                Console.WriteLine(func());
            }
        }
    }
}
