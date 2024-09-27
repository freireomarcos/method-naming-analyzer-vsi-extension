using DevGuidance.Analyzers.Analyzers;
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

namespace DevGuidanceLint.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncMethodNameAnalyzerCodeFixProvider)), Shared]
    public class AsyncMethodNameAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add 'Async' Sufix";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AsyncMethodNameAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var methodDeclaration = root.FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .First();

            context.RegisterCodeFix(CodeAction.Create(title: Title, createChangedSolution: c => AddAsyncSuffixAsync(context.Document, methodDeclaration, c), equivalenceKey: Title), diagnostic);
        }

        private async Task<Solution> AddAsyncSuffixAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
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