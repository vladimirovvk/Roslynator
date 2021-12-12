#region usings
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslynator.Testing;
using Roslynator.Testing.CSharp;
using Roslynator.Testing.CSharp.Xunit;
#endregion usings

#pragma warning disable RCS1090

namespace Sample.Tests
{
    public abstract class AbstractCSharpRefactoringVerifier<TRefactoringProvider> : XunitRefactoringVerifier<TRefactoringProvider>
        where TRefactoringProvider : CodeRefactoringProvider, new()
    {
        public override CSharpTestOptions Options => DefaultCSharpTestOptions.Value;

        public async Task VerifyRefactoringAsync(
            string source,
            string expectedSource,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source);

            Debug.Assert(code.Spans.Length > 0);

            var expected = ExpectedTestState.Parse(expectedSource);

            var data = new RefactoringTestData(
                code.Value,
                code.Spans.OrderByDescending(f => f.Start).ToImmutableArray(),
                AdditionalFile.CreateRange(additionalFiles),
                equivalenceKey: equivalenceKey);

            await VerifyRefactoringAsync(
                data,
                expected,
                options,
                cancellationToken: cancellationToken);
        }

        public async Task VerifyRefactoringAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source, sourceData, expectedData);

            Debug.Assert(code.Spans.Length > 0);

            var expected = ExpectedTestState.Parse(code.ExpectedValue);

            var data = new RefactoringTestData(
                code.Value,
                code.Spans.OrderByDescending(f => f.Start).ToImmutableArray(),
                AdditionalFile.CreateRange(additionalFiles),
                equivalenceKey: equivalenceKey);

            await VerifyRefactoringAsync(
                data,
                expected,
                options,
                cancellationToken: cancellationToken);
        }

        public async Task VerifyNoRefactoringAsync(
            string source,
            string equivalenceKey = null,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source);

            var data = new RefactoringTestData(
                code.Value,
                code.Spans,
                equivalenceKey: equivalenceKey);

            await VerifyNoRefactoringAsync(
                data,
                options,
                cancellationToken: cancellationToken);
        }
    }
}
