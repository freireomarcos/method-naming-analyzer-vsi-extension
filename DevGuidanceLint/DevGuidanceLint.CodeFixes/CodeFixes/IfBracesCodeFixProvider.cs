using DevGuidance.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace DevGuidanceLint.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IfBracesCodeFixProvider)), Shared]
    public class IfBracesCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add braces to 'if' statement";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(IfBracesAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var ifStatement = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<IfStatementSyntax>();

            context.RegisterCodeFix(CodeAction.Create(Title, c => AddBracesAsync(context.Document, ifStatement, c), equivalenceKey: Title), diagnostic);
        }

        private async Task<Document> AddBracesAsync(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var newIfStatement = ifStatement.WithStatement(SyntaxFactory.Block(ifStatement.Statement)).WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(ifStatement, newIfStatement);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
