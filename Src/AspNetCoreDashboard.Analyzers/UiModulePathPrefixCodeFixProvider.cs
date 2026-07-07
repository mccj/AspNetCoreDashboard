using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AspNetCoreDashboard.Analyzers
{
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UiModulePathPrefixCodeFixProvider)), Shared]
  public sealed class UiModulePathPrefixCodeFixProvider : CodeFixProvider
  {
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(UiModuleAnalyzer.InvalidPathPrefixRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      if (root == null)
        return;

      foreach (var diagnostic in context.Diagnostics)
      {
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var propertyDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (propertyDeclaration == null)
          continue;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "在 PathPrefix 前添加 '/'",
                createChangedDocument: cancellationToken =>
                    FixPathPrefixAsync(context.Document, propertyDeclaration, cancellationToken),
                equivalenceKey: "PrependSlashToPathPrefix"),
            diagnostic);
      }
    }

    private static async Task<Document> FixPathPrefixAsync(
        Document document,
        PropertyDeclarationSyntax propertyDeclaration,
        CancellationToken cancellationToken)
    {
      var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
      if (root == null)
        return document;

      var newProperty = propertyDeclaration.ReplaceNode(
          GetPathPrefixExpression(propertyDeclaration),
          FixExpression(GetPathPrefixExpression(propertyDeclaration)));

      return document.WithSyntaxRoot(root.ReplaceNode(propertyDeclaration, newProperty));
    }

    private static ExpressionSyntax GetPathPrefixExpression(PropertyDeclarationSyntax propertyDeclaration)
    {
      if (propertyDeclaration.ExpressionBody != null)
        return propertyDeclaration.ExpressionBody.Expression;

      var getter = propertyDeclaration.AccessorList?.Accessors
          .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
      return getter?.ExpressionBody?.Expression
          ?? throw new System.InvalidOperationException("PathPrefix 必须使用表达式体语法。");
    }

    private static ExpressionSyntax FixExpression(ExpressionSyntax expression)
    {
      if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
      {
        var value = literal.Token.ValueText;
        if (!value.StartsWith("/"))
        {
          return SyntaxFactory.LiteralExpression(
              SyntaxKind.StringLiteralExpression,
              SyntaxFactory.Literal("/" + value));
        }
      }

      return expression;
    }
  }
}
