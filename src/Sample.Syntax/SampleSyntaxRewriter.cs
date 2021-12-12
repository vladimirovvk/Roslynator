#region usings
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#endregion usings

namespace Sample
{
    internal class SampleSyntaxRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            return node.WithIdentifier(SyntaxFactory.Identifier("C2").WithTriviaFrom(node.Identifier));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return node.WithIdentifier(SyntaxFactory.Identifier("M2").WithTriviaFrom(node.Identifier));
        }
    }
}
