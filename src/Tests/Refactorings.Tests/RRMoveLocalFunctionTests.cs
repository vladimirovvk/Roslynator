// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class RRMoveLocalFunctionTests : AbstractCSharpRefactoringVerifier
    {
        public override string RefactoringId { get; } = RefactoringIdentifiers.MoveLocalFunction;

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.MoveLocalFunction)]
        public async Task Test()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        void [||]LF()
        {
        }

        M();
        M();
    }
}
", @"
class C
{
    void M()
    {

        M();
        M();
        void LF()
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }

        [Fact, Trait(Traits.Refactoring, RefactoringIdentifiers.MoveLocalFunction)]
        public async Task Test2()
        {
            await VerifyRefactoringAsync(@"
class C
{
    void M()
    {
        void [||]LF()
        {
        }

        M();
        M();

        void LF2()
        {
        }
    }
}
", @"
class C
{
    void M()
    {

        M();
        M();
        void LF()
        {
        }

        void LF2()
        {
        }
    }
}
", equivalenceKey: RefactoringId);
        }
    }
}
