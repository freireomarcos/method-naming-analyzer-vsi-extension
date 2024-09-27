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
    public class AsyncMethodNameAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = CategoryConstants.NAMING;
        public const string DiagnosticId = DiagnosticIdsConstants.ASYNC_NAMING_ANALYZER_DIAGNOSTIC_ID;

        private static readonly string Title = "All async methods should end with 'Async' suffix.";
        private static readonly string Description = "All async methods should end with 'Async' suffix.";
        private static readonly string MessageFormat = "Method '{0}' is async but does not end with 'Async'";
        
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return;
            }

            if (IsInController(methodDeclaration))
            {
                return;
            }

            if (!methodDeclaration.Identifier.Text.EndsWith("Async"))
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsInController(MethodDeclarationSyntax methodDeclaration)
        {
            var classDeclaration = methodDeclaration.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            return classDeclaration != null && classDeclaration.Identifier.Text.EndsWith("Controller");
        }
    }
}
