using DevGuidance.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReturnWithoutVariableCodeFixProvider)), Shared]
public class ReturnWithoutVariableCodeFixProvider : CodeFixProvider
{   
    private const string Title = "Assign return value to a variable";

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ReturnWithoutVariableAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var returnStatement = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<ReturnStatementSyntax>();

        context.RegisterCodeFix(CodeAction.Create(Title, c => IntroduceVariableAsync(context.Document, returnStatement, c), equivalenceKey: Title), diagnostic);
    }

    private async Task<Document> IntroduceVariableAsync(Document document, ReturnStatementSyntax returnStatement, CancellationToken cancellationToken)
    {
        var expression = returnStatement.Expression;

        var methodDeclaration = returnStatement.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration == null)
        {
            return document;
        }

        var variableName = "response";
        var returnType = methodDeclaration.ReturnType;

        var variable = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(variableName))
            .WithInitializer(SyntaxFactory.EqualsValueClause(expression));

        var syntaxDeclaration = SyntaxFactory.VariableDeclaration(returnType)
            .WithVariables(SyntaxFactory.SingletonSeparatedList(variable));

        var variableDeclaration = SyntaxFactory.LocalDeclarationStatement(syntaxDeclaration)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newReturnStatement = returnStatement
            .WithExpression(SyntaxFactory.IdentifierName(variableName))
            .WithAdditionalAnnotations(Formatter.Annotation);

        if (returnStatement.Parent is BlockSyntax parentBlock)
        {
            var newStatements = new SyntaxList<StatementSyntax>();
            newStatements = newStatements.Add(variableDeclaration);

            foreach (var statement in parentBlock.Statements)
            {
                if (statement == returnStatement)
                {
                    newStatements = newStatements.Add(newReturnStatement);
                }
                else
                {
                    newStatements = newStatements.Add(statement);
                }
            }

            var newBlock = parentBlock.WithStatements(newStatements);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(parentBlock, newBlock);

            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
}