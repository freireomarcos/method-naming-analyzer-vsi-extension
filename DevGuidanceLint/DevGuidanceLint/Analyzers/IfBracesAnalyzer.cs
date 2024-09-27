using DevGuidance.Analyzers.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DevGuidance.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IfBracesAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = CategoryConstants.CODE_STYLE;
        public const string DiagnosticId = DiagnosticIdsConstants.IF_BRACES_ANALYZER_DIAGNOSTIC_ID;

        private static readonly string Title = "If statement should have braces";
        private static readonly string MessageFormat = "The 'if' statement should use braces.";
        private static readonly string Description = "Ensure all 'if' statements have braces.";

        private readonly static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (!(ifStatement.Statement is BlockSyntax))
            {
                var diagnostic = Diagnostic.Create(Rule, ifStatement.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
