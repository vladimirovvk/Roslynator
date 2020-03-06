// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RCS1213

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FormattingAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.RemoveUnnecessaryNewLine); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                    return;

                startContext.RegisterSyntaxNodeAction(AnalyzeNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeStructDeclaration, SyntaxKind.StructDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeEnumDeclaration, SyntaxKind.EnumDeclaration);

                startContext.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.GetAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.SetAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.AddAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeAccessorDeclaration, SyntaxKind.RemoveAccessorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeConversionOperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeDelegateDeclaration, SyntaxKind.DelegateDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeDestructorDeclaration, SyntaxKind.DestructorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeEventDeclaration, SyntaxKind.EventDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeEventFieldDeclaration, SyntaxKind.EventFieldDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeIndexerDeclaration, SyntaxKind.IndexerDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzeOperatorDeclaration, SyntaxKind.OperatorDeclaration);
                startContext.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);

                startContext.RegisterSyntaxNodeAction(AnalyzeLocalDeclarationStatement, SyntaxKind.LocalDeclarationStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeEmptyStatement, SyntaxKind.EmptyStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeLabeledStatement, SyntaxKind.LabeledStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeGotoStatement, SyntaxKind.GotoStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeGotoCaseStatement, SyntaxKind.GotoCaseStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeGotoDefaultStatement, SyntaxKind.GotoDefaultStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeBreakStatement, SyntaxKind.BreakStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeContinueStatement, SyntaxKind.ContinueStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeYieldReturnStatement, SyntaxKind.YieldReturnStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeYieldBreakStatement, SyntaxKind.YieldBreakStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeDoStatement, SyntaxKind.DoStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeCommonForEachStement, SyntaxKind.ForEachStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeFixedStatement, SyntaxKind.FixedStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeCheckedStatement, SyntaxKind.CheckedStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeUncheckedStatement, SyntaxKind.UncheckedStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeUnsafeStatement, SyntaxKind.UnsafeStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeTryStatement, SyntaxKind.TryStatement);
                startContext.RegisterSyntaxNodeAction(AnalyzeLocalFunctionStatement, SyntaxKind.LocalFunctionStatement);
                //startContext.RegisterSyntaxNodeAction(AnalyzeCommonForEachStement, SyntaxKind.ForEachVariableStatement);

                startContext.RegisterSyntaxNodeAction(AnalyzeCasePatternSwitchLabel, SyntaxKind.CasePatternSwitchLabel);
                startContext.RegisterSyntaxNodeAction(AnalyzeCaseSwitchLabel, SyntaxKind.CaseSwitchLabel);
                startContext.RegisterSyntaxNodeAction(AnalyzeDefaultSwitchLabel, SyntaxKind.DefaultSwitchLabel);

                startContext.RegisterSyntaxNodeAction(AnalyzeNameColon, SyntaxKind.NameColon);
                startContext.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);

                startContext.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
            });
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken closingBrace = namespaceDeclaration.CloseBraceToken;

                if (!closingBrace.IsMissing)
                {
                    SyntaxToken semicolon = namespaceDeclaration.SemicolonToken;

                    if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                        AnalyzeUnnecessaryNewLine(context, closingBrace, semicolon);
                }
            }
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeUnnecessaryNewLine(context, classDeclaration);
        }

        private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeUnnecessaryNewLine(context, interfaceDeclaration);
        }

        private static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeUnnecessaryNewLine(context, structDeclaration);
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeUnnecessaryNewLine(context, enumDeclaration);
        }

        private static void AnalyzeAccessorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var accessorDeclaration = (AccessorDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, accessorDeclaration.Modifiers);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, constructorDeclaration.Modifiers);
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, conversionOperatorDeclaration.Modifiers);
        }

        private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, delegateDeclaration.Modifiers);
        }

        private static void AnalyzeDestructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var destructorDeclaration = (DestructorDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, destructorDeclaration.Modifiers);
        }

        private static void AnalyzeEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventDeclaration = (EventDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, eventDeclaration.Modifiers);
        }

        private static void AnalyzeEventFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventFieldDeclaration = (EventFieldDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, eventFieldDeclaration.Modifiers);
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, fieldDeclaration.Modifiers);
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                AnalyzeModifiers(context, indexerDeclaration.Modifiers);
                AnalyzeAccessorList(context, indexerDeclaration.ParameterList.CloseBracketToken, indexerDeclaration.AccessorList);
            }
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, methodDeclaration.Modifiers);
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
                AnalyzeModifiers(context, operatorDeclaration.Modifiers);
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                AnalyzeModifiers(context, propertyDeclaration.Modifiers);
                AnalyzeAccessorList(context, propertyDeclaration.Identifier, propertyDeclaration.AccessorList);
            }
        }

        private static void AnalyzeLocalDeclarationStatement(SyntaxNodeAnalysisContext context)
        {
            var localDeclarationStatement = (LocalDeclarationStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                AnalyzeModifiers(context, localDeclarationStatement.Modifiers);

                VariableDeclarationSyntax variableDeclaration = localDeclarationStatement.Declaration;

                SyntaxToken semicolon = localDeclarationStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, variableDeclaration, semicolon);
            }
        }

        private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
        {
            var expressionStatement = (ExpressionStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                ExpressionSyntax expression = expressionStatement.Expression;

                SyntaxToken semicolon = expressionStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, expression, semicolon);
            }
        }

        private static void AnalyzeEmptyStatement(SyntaxNodeAnalysisContext context)
        {
            var emptyStatement = (EmptyStatementSyntax)context.Node;
        }

        private static void AnalyzeLabeledStatement(SyntaxNodeAnalysisContext context)
        {
            var labeledStatement = (LabeledStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken token = labeledStatement.Identifier;

                SyntaxToken colon = labeledStatement.ColonToken;

                if (colon.IsKind(SyntaxKind.ColonToken))
                    AnalyzeUnnecessaryNewLine(context, token, colon);
            }
        }

        private static void AnalyzeGotoStatement(SyntaxNodeAnalysisContext context)
        {
            var gotoStatement = (GotoStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                ExpressionSyntax expression = gotoStatement.Expression;

                if (expression != null)
                {
                    SyntaxToken semicolon = gotoStatement.SemicolonToken;

                    if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                        AnalyzeUnnecessaryNewLine(context, expression, semicolon);
                }
            }
        }

        private static void AnalyzeGotoCaseStatement(SyntaxNodeAnalysisContext context)
        {
            var gotoStatement = (GotoStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                ExpressionSyntax expression = gotoStatement.Expression;

                if (expression != null)
                {
                    SyntaxToken semicolon = gotoStatement.SemicolonToken;

                    if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                        AnalyzeUnnecessaryNewLine(context, expression, semicolon);
                }
            }
        }

        private static void AnalyzeGotoDefaultStatement(SyntaxNodeAnalysisContext context)
        {
            var gotoStatement = (GotoStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken deafultKeyword = gotoStatement.CaseOrDefaultKeyword;

                SyntaxToken semicolon = gotoStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, deafultKeyword, semicolon);
            }
        }

        private static void AnalyzeBreakStatement(SyntaxNodeAnalysisContext context)
        {
            var breakStatement = (BreakStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken breakKeyword = breakStatement.BreakKeyword;

                SyntaxToken semicolon = breakStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, breakKeyword, semicolon);
            }
        }

        private static void AnalyzeContinueStatement(SyntaxNodeAnalysisContext context)
        {
            var continueStatement = (ContinueStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken continueKeyword = continueStatement.ContinueKeyword;

                SyntaxToken semicolon = continueStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, continueKeyword, semicolon);
            }
        }

        private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxNodeOrToken nodeOrToken = returnStatement.Expression ?? (SyntaxNodeOrToken)returnStatement.ReturnKeyword;

                SyntaxToken semicolon = returnStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, nodeOrToken, semicolon);
            }
        }

        private static void AnalyzeYieldReturnStatement(SyntaxNodeAnalysisContext context)
        {
            var yieldStatement = (YieldStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                ExpressionSyntax expression = yieldStatement.Expression;

                if (expression != null)
                {
                    SyntaxToken semicolon = yieldStatement.SemicolonToken;

                    if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                        AnalyzeUnnecessaryNewLine(context, expression, semicolon);
                }
            }
        }

        private static void AnalyzeYieldBreakStatement(SyntaxNodeAnalysisContext context)
        {
            var yieldStatement = (YieldStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken breakKeyword = yieldStatement.ReturnOrBreakKeyword;

                SyntaxToken semicolon = yieldStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, breakKeyword, semicolon);
            }
        }

        private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxNodeOrToken nodeOrToken = throwStatement.Expression ?? (SyntaxNodeOrToken)throwStatement.ThrowKeyword;

                SyntaxToken semicolon = throwStatement.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, nodeOrToken, semicolon);
            }
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken closeParen = doStatement.CloseParenToken;

                if (!closeParen.IsMissing)
                {
                    SyntaxToken semicolon = doStatement.SemicolonToken;

                    if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                        AnalyzeUnnecessaryNewLine(context, closeParen, semicolon);
                }
            }
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var forStatement = (ForStatementSyntax)context.Node;
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;
        }

        private static void AnalyzeFixedStatement(SyntaxNodeAnalysisContext context)
        {
            var fixedStatement = (FixedStatementSyntax)context.Node;
        }

        private static void AnalyzeCheckedStatement(SyntaxNodeAnalysisContext context)
        {
            var checkedStatement = (CheckedStatementSyntax)context.Node;
        }

        private static void AnalyzeUncheckedStatement(SyntaxNodeAnalysisContext context)
        {
            var checkedStatement = (CheckedStatementSyntax)context.Node;
        }

        private static void AnalyzeUnsafeStatement(SyntaxNodeAnalysisContext context)
        {
            var unsafeStatement = (UnsafeStatementSyntax)context.Node;
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var lockStatement = (LockStatementSyntax)context.Node;
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;
        }

        private static void AnalyzeTryStatement(SyntaxNodeAnalysisContext context)
        {
            var tryStatement = (TryStatementSyntax)context.Node;
        }

        private static void AnalyzeLocalFunctionStatement(SyntaxNodeAnalysisContext context)
        {
            var localFunctionStatement = (LocalFunctionStatementSyntax)context.Node;

            AnalyzeModifiers(context, localFunctionStatement.Modifiers);
        }

        private static void AnalyzeCommonForEachStement(SyntaxNodeAnalysisContext context)
        {
            var forEachStatement = (CommonForEachStatementSyntax)context.Node;
        }

        private static void AnalyzeCasePatternSwitchLabel(SyntaxNodeAnalysisContext context)
        {
            var casePatternSwitchLabel = (CasePatternSwitchLabelSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxNode node = casePatternSwitchLabel.WhenClause ?? (SyntaxNode)casePatternSwitchLabel.Pattern;

                if (node != null)
                {
                    SyntaxToken colon = casePatternSwitchLabel.ColonToken;

                    if (colon.IsKind(SyntaxKind.ColonToken))
                        AnalyzeUnnecessaryNewLine(context, node, colon);
                }
            }
        }

        private static void AnalyzeCaseSwitchLabel(SyntaxNodeAnalysisContext context)
        {
            var caseSwitchLabel = (CaseSwitchLabelSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                ExpressionSyntax value = caseSwitchLabel.Value;

                if (value != null)
                {
                    SyntaxToken colon = caseSwitchLabel.ColonToken;

                    if (colon.IsKind(SyntaxKind.ColonToken))
                        AnalyzeUnnecessaryNewLine(context, value, colon);
                }
            }
        }

        private static void AnalyzeDefaultSwitchLabel(SyntaxNodeAnalysisContext context)
        {
            var defaultSwitchLabel = (DefaultSwitchLabelSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                SyntaxToken defaultKeyword = defaultSwitchLabel.Keyword;

                SyntaxToken colon = defaultSwitchLabel.ColonToken;

                if (colon.IsKind(SyntaxKind.ColonToken))
                    AnalyzeUnnecessaryNewLine(context, defaultKeyword, colon);
            }
        }

        private static void AnalyzeNameColon(SyntaxNodeAnalysisContext context)
        {
            var nameColon = (NameColonSyntax)context.Node;

            if (!context.IsAnalyzerSuppressed(DiagnosticDescriptors.RemoveUnnecessaryNewLine))
            {
                IdentifierNameSyntax name = nameColon.Name;

                SyntaxToken colon = nameColon.ColonToken;

                if (colon.IsKind(SyntaxKind.ColonToken))
                    AnalyzeUnnecessaryNewLine(context, name, colon);
            }
        }

        private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
        {
            var parameter = (ParameterSyntax)context.Node;

            AnalyzeModifiers(context, parameter.Modifiers);
        }

        private static void AnalyzeUnnecessaryNewLine(SyntaxNodeAnalysisContext context, BaseTypeDeclarationSyntax declaration)
        {
            AnalyzeModifiers(context, declaration.Modifiers);

            SyntaxToken closingBrace = declaration.CloseBraceToken;

            if (!closingBrace.IsMissing)
            {
                SyntaxToken semicolon = declaration.SemicolonToken;

                if (semicolon.IsKind(SyntaxKind.SemicolonToken))
                    AnalyzeUnnecessaryNewLine(context, closingBrace, semicolon);
            }
        }

        private static void AnalyzeModifiers(SyntaxNodeAnalysisContext context, SyntaxTokenList modifiers)
        {
            if (!modifiers.Any())
                return;

            SyntaxToken modifier1 = modifiers[0];

            for (int i = 1; i < modifiers.Count; i++)
            {
                SyntaxToken modifier2 = modifiers[i];
                AnalyzeUnnecessaryNewLine(context, modifier1, modifier2);
                modifier1 = modifier2;
            }

            SyntaxToken last = modifiers.Last();

            AnalyzeUnnecessaryNewLine(context, last, last.GetNextToken());
        }

        private static void AnalyzeAccessorList(SyntaxNodeAnalysisContext context, SyntaxToken token, AccessorListSyntax accessorList)
        {
            if (accessorList.Accessors.All(f => f.BodyOrExpressionBody() == null)
                && accessorList.IsSingleLine())
            {
                AnalyzeUnnecessaryNewLine(context, token, accessorList);
            }
        }

        private static void AnalyzeUnnecessaryNewLine(SyntaxNodeAnalysisContext context, SyntaxNodeOrToken nodeOrToken1, SyntaxNodeOrToken nodeOrToken2)
        {
            if (SyntaxTriviaAnalysis.IsOptionalWhitespaceThenEndOfLineTrivia(nodeOrToken1.GetTrailingTrivia())
                && nodeOrToken2.GetLeadingTrivia().IsEmptyOrWhitespace())
            {
                ReportDiagnostic(context, nodeOrToken1);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNodeOrToken nodeOrToken)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.RemoveUnnecessaryNewLine,
                Location.Create(nodeOrToken.SyntaxTree, new TextSpan(nodeOrToken.Span.End, 0)));
        }
    }
}
