#region usings
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
#endregion usings

#pragma warning disable RCS1090, RCS0053

namespace Sample.Tests
{
    public class SampleAnalyzerTests : AbstractCSharpDiagnosticVerifier<SampleAnalyzer, SampleCodeFixProvider>
    {
        public override DiagnosticDescriptor Descriptor { get; } = SampleAnalyzer.Descriptor;

        [Fact]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"[||]
class C
{
    void M()
    {
    }
}
", @"// aaa

class C
{
    void M()
    {
    }
}
", options: Options.WithConfigOptions(Options.ConfigOptions.Add(SampleAnalyzer.OptionKey, "aaa")));
        }

        [Fact]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"// aaa
class C
{
    void M()
    {
    }
}
", options: Options.WithConfigOptions(Options.ConfigOptions.Add(SampleAnalyzer.OptionKey, "aaa")));
        }
    }
}
