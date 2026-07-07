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
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UiModuleAttributePathPrefixCodeFixProvider)), Shared]
  public sealed class UiModuleAttributePathPrefixCodeFixProvider : CodeFixProvider
  {
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(UiModuleAnalyzer.PathPrefixAttributeMismatchRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
      var document = context.Document;
      var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
      var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
      if (root == null || semanticModel == null)
        return;

      foreach (var diagnostic in context.Diagnostics)
      {
        var propertyDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (propertyDeclaration == null)
          continue;

        var typeDeclaration = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDeclaration == null)
          continue;

        var attributePrefix = GetUiModuleAttributePathPrefix(typeDeclaration, semanticModel);
        if (string.IsNullOrEmpty(attributePrefix))
          continue;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "将 PathPrefix 与 UiModuleAttribute 对齐",
                createChangedDocument: cancellationToken =>
                    FixPathPrefixAsync(document, propertyDeclaration, attributePrefix, cancellationToken),
                equivalenceKey: "AlignPathPrefixWithUiModuleAttribute"),
            diagnostic);
      }
    }

    private static string GetUiModuleAttributePathPrefix(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
    {
      var syntaxAttribute = typeDeclaration.AttributeLists
          .SelectMany(list => list.Attributes)
          .FirstOrDefault(a => a.Name.ToString().IndexOf("UiModule", System.StringComparison.Ordinal) >= 0);

      if (syntaxAttribute?.ArgumentList?.Arguments.Count > 0)
      {
        var constant = semanticModel.GetConstantValue(syntaxAttribute.ArgumentList.Arguments[0].Expression);
        if (constant.HasValue)
          return constant.Value as string;
      }

      return null;
    }

    private static async Task<Document> FixPathPrefixAsync(
        Document document,
        PropertyDeclarationSyntax propertyDeclaration,
        string attributePrefix,
        CancellationToken cancellationToken)
    {
      var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
      if (root == null)
        return document;

      var expression = propertyDeclaration.ExpressionBody?.Expression
          ?? propertyDeclaration.AccessorList?.Accessors
              .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration))?
              .ExpressionBody?.Expression;

      if (expression == null)
        return document;

      var newExpression = SyntaxFactory.LiteralExpression(
          SyntaxKind.StringLiteralExpression,
          SyntaxFactory.Literal(attributePrefix));

      var newProperty = propertyDeclaration.ReplaceNode(expression, newExpression);
      return document.WithSyntaxRoot(root.ReplaceNode(propertyDeclaration, newProperty));
    }
  }
}
