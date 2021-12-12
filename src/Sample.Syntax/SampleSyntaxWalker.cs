#region usings
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Sample.MyConsole;
#endregion usings

#pragma warning disable RCS1090

namespace Sample
{
    internal class SampleSyntaxWalker : CSharpSyntaxWalker
    {
        private int _depth;

        public SemanticModel SemanticModel { get; }

        public SampleSyntaxWalker(SemanticModel semanticModel) : base(SyntaxWalkerDepth.Token)
        {
            SemanticModel = semanticModel;
        }

        public override void Visit(SyntaxNode node)
        {
            Write(new string(' ', _depth * 2) + node.Kind().ToString(), ConsoleColor.Blue);

            ISymbol symbol = SemanticModel.GetDeclaredSymbol(node);

            if (symbol != null)
            {
                Write(" declared symbol: ", ConsoleColor.DarkGray);
                Write(symbol.ToDisplayString(), ConsoleColor.Yellow);
            }
            else
            {
                symbol = SemanticModel.GetSymbolInfo(node).Symbol;

                if (symbol != null)
                {
                    Write(" symbol: ", ConsoleColor.DarkGray);
                    Write(symbol.ToDisplayString(), ConsoleColor.Yellow);
                }

                ITypeSymbol typeSymbol = SemanticModel.GetTypeInfo(node).Type;

                if (typeSymbol != null)
                {
                    Write(" type: ", ConsoleColor.DarkGray);
                    Write(typeSymbol.ToDisplayString(), ConsoleColor.Yellow);
                }
            }

            if (symbol != null)
            {
                Write(" kind: ", ConsoleColor.DarkGray);
                Write(symbol.Kind, ConsoleColor.Yellow);
            }

            WriteLine();
            _depth++;
            base.Visit(node);
            _depth--;
        }

        public override void VisitToken(SyntaxToken token)
        {
            Write(new string(' ', _depth * 2) + token.Kind().ToString(), ConsoleColor.Green);
            Write(" " + token.ToString(), ConsoleColor.White);
            WriteLine();

            _depth++;
            base.VisitToken(token);
            _depth--;
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            WriteLine(new string(' ', _depth * 2) + trivia.Kind().ToString(), ConsoleColor.Red);

            _depth++;
            base.VisitTrivia(trivia);
            _depth--;
        }
    }
}
