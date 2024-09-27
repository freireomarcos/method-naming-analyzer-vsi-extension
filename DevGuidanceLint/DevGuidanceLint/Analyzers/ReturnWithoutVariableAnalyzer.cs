using DevGuidance.Analyzers.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace DevGuidance.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReturnWithoutVariableAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = CategoryConstants.READABILITY;
        public const string DiagnosticId = DiagnosticIdsConstants.RETURN_WITHOUT_VARIABLE_ANALYZER_DISGNOSTIC_ID;

        private const string Title = "Return without assignment to a variable";
        private const string MessageFormat = "The return value was not assigned to a variable before being returned";
        private const string Description = "The return value of a method should be assigned to a variable before being returned to improve readability";

        private readonly static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.ReturnStatement);
        }

        private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;

            var expression = returnStatement.Expression;

            var notAllowedExpressionTypes = new[]
            {
                typeof(ObjectCreationExpressionSyntax),
                typeof(InvocationExpressionSyntax),
                typeof(LiteralExpressionSyntax)
            };

            if (notAllowedExpressionTypes.Any(t => t.IsInstanceOfType(expression)))
            {
                var diagnostic = Diagnostic.Create(Rule, returnStatement.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
