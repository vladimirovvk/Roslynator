#region usings
using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;
#endregion usings

#pragma warning disable RCS1090, RCS0053

namespace Sample.Tests
{
    public class SampleRefactoringTest : AbstractCSharpRefactoringVerifier<SampleCodeRefactoringProvider>
    {
        public override CSharpTestOptions Options => CSharpTestOptions.Default;

        [Fact]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
using System;

class C
{
    void M()
    {
        [||]var x = System.TimeSpan.FromHours(1);
    }
}
", @"
using System;

class C
{
    void M()
    {
        TimeSpan x = System.TimeSpan.FromHours(1);
    }
}
");
        }
    }
}
