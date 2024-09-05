using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MethodNamingAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NamingAnalyzerCodeFixProvider)), Shared]
    public class NamingAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add 'Async' suffix";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NamingAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            context.RegisterCodeFix(CodeAction.Create(title: NamingAnalyzerAnalyzer.DiagnosticId, createChangedSolution: c => AddAsyncSuffix(context.Document, methodDeclaration, c),equivalenceKey: Title),diagnostic);
        }

        private async Task<Solution> AddAsyncSuffix(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {   
            var newName = methodDeclaration.Identifier.Text + "Async";

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var symbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);

            var solution = document.Project.Solution;
            var optionSet = solution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            return newSolution;
        }
    }
}