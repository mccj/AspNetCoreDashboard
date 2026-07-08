using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AspNetCoreDashboard.Analyzers
{
  /// <summary>注册 AspNetCoreDashboard 分析器。</summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public sealed class UiModuleAnalyzer : DiagnosticAnalyzer
  {
    /// <summary>PathPrefix 必须以 '/' 开头。</summary>
    public static readonly DiagnosticDescriptor InvalidPathPrefixRule = new DiagnosticDescriptor(
        id: "ACD001",
        title: "UI 模块 PathPrefix 必须以 '/' 开头",
        messageFormat: "PathPrefix '{0}' 必须以 '/' 开头",
        category: "AspNetCoreDashboard",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>MapFallbackToIndex 指定的资源必须存在于嵌入式资源中。</summary>
    public static readonly DiagnosticDescriptor MissingFallbackResourceRule = new DiagnosticDescriptor(
        id: "ACD002",
        title: "找不到回退 index 资源",
        messageFormat: "未找到嵌入式资源 '{0}'。请确认 MapFallbackToIndex 与 EmbeddedResource 的 LogicalName 一致。",
        category: "AspNetCoreDashboard",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>MapEmbeddedUi 的命名空间应以 '.Content' 结尾。</summary>
    public static readonly DiagnosticDescriptor InvalidEmbeddedNamespaceRule = new DiagnosticDescriptor(
        id: "ACD003",
        title: "嵌入式 UI 命名空间应以 '.Content' 结尾",
        messageFormat: "MapEmbeddedUi 命名空间 '{0}' 通常应以 '.Content' 结尾（例如 '{1}.Content'）。",
        category: "AspNetCoreDashboard",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>同一 HTTP 方法的路由模式重复。</summary>
    public static readonly DiagnosticDescriptor DuplicateRouteRule = new DiagnosticDescriptor(
        id: "ACD004",
        title: "UI 模块路由重复",
        messageFormat: "Configure() 中 {1} 路由 '{0}' 被注册了多次。",
        category: "AspNetCoreDashboard",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>SPA 回退但未配置嵌入式 UI。</summary>
    public static readonly DiagnosticDescriptor SpaFallbackWithoutEmbeddedUiRule = new DiagnosticDescriptor(
        id: "ACD005",
        title: "SPA 回退需要先调用 MapEmbeddedUi",
        messageFormat: "请在 MapFallbackToIndex 或 MapSpaFallback 之前调用 MapEmbeddedUi。",
        category: "AspNetCoreDashboard",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>PathPrefix 属性与 UiModuleAttribute 不一致。</summary>
    public static readonly DiagnosticDescriptor PathPrefixAttributeMismatchRule = new DiagnosticDescriptor(
        id: "ACD006",
        title: "PathPrefix 与 UiModuleAttribute 不一致",
        messageFormat: "PathPrefix '{0}' 与 [UiModule(\"{1}\")] 不匹配。",
        category: "AspNetCoreDashboard",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>带有 UiModule 特性但未调用 MapEmbeddedUi。</summary>
    public static readonly DiagnosticDescriptor MissingEmbeddedUiRule = new DiagnosticDescriptor(
        id: "ACD007",
        title: "带 UiModule 特性的模块应调用 MapEmbeddedUi",
        messageFormat: "模块 '{0}' 带有 [UiModule]，但 Configure() 未调用 MapEmbeddedUi。",
        category: "AspNetCoreDashboard",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            InvalidPathPrefixRule,
            MissingFallbackResourceRule,
            InvalidEmbeddedNamespaceRule,
            DuplicateRouteRule,
            SpaFallbackWithoutEmbeddedUiRule,
            PathPrefixAttributeMismatchRule,
            MissingEmbeddedUiRule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();
      context.RegisterSyntaxNodeAction(AnalyzePathPrefixProperty, SyntaxKind.PropertyDeclaration);
      context.RegisterCompilationStartAction(compilationContext =>
      {
        var embeddedResources = GetEmbeddedResourceNames(compilationContext.Options.AdditionalFiles);
        compilationContext.RegisterSyntaxNodeAction(
                  ctx => AnalyzeConfigureMethod(ctx, embeddedResources),
                  SyntaxKind.MethodDeclaration);
      });
    }

    private static void AnalyzePathPrefixProperty(SyntaxNodeAnalysisContext context)
    {
      var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
      if (propertyDeclaration.Identifier.Text != "PathPrefix")
        return;

      var typeDeclaration = propertyDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();
      if (typeDeclaration == null)
        return;

      var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
      if (typeSymbol == null || !ImplementsUiModule(typeSymbol))
        return;

      var pathPrefix = GetConstantString(propertyDeclaration, context.SemanticModel);
      if (pathPrefix != null && !pathPrefix.StartsWith("/"))
      {
        context.ReportDiagnostic(Diagnostic.Create(
            InvalidPathPrefixRule,
            propertyDeclaration.GetLocation(),
            pathPrefix));
      }

      var attributePrefix = GetUiModuleAttributePathPrefix(typeSymbol, typeDeclaration, context.SemanticModel);
      if (pathPrefix != null && attributePrefix != null &&
          !string.Equals(pathPrefix.TrimEnd('/'), attributePrefix.TrimEnd('/'), System.StringComparison.OrdinalIgnoreCase))
      {
        context.ReportDiagnostic(Diagnostic.Create(
            PathPrefixAttributeMismatchRule,
            propertyDeclaration.GetLocation(),
            pathPrefix,
            attributePrefix));
      }
    }

    private static string GetUiModuleAttributePathPrefix(
        INamedTypeSymbol typeSymbol,
        TypeDeclarationSyntax typeDeclaration,
        SemanticModel semanticModel)
    {
      var attribute = typeSymbol.GetAttributes()
          .FirstOrDefault(a =>
              a.AttributeClass?.Name == "UiModuleAttribute" ||
              a.AttributeClass?.Name == "UiModule");
      if (attribute != null && attribute.ConstructorArguments.Length > 0)
        return attribute.ConstructorArguments[0].Value as string;

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

    private static void AnalyzeConfigureMethod(SyntaxNodeAnalysisContext context, HashSet<string> embeddedResources)
    {
      var methodDeclaration = (MethodDeclarationSyntax)context.Node;
      if (methodDeclaration.Identifier.Text != "Configure")
        return;

      var typeDeclaration = methodDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();
      if (typeDeclaration == null)
        return;

      var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
      if (typeSymbol == null || !ImplementsUiModule(typeSymbol))
        return;

      var routeKeys = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
      var hasEmbeddedUi = false;
      var hasSpaFallback = false;
      InvocationExpressionSyntax spaFallbackInvocation = null;

      foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
      {
        var methodName = GetSimpleMethodName(invocation);
        if (methodName == null)
          continue;

        if (methodName == "MapEmbeddedUi")
        {
          hasEmbeddedUi = true;
          AnalyzeMapEmbeddedUi(context, invocation, typeSymbol);
          continue;
        }

        if (methodName == "MapFallbackToIndex" || methodName == "MapSpaFallback")
        {
          hasSpaFallback = true;
          spaFallbackInvocation = invocation;
          AnalyzeMapFallbackToIndex(context, invocation, embeddedResources);
          continue;
        }

        if (!TryGetRouteMethod(methodName, out var httpMethod))
          continue;

        var pattern = GetInvocationStringLiteral(invocation, context.SemanticModel);
        if (pattern == null)
          continue;

        var routeKey = httpMethod + "|" + pattern;
        if (!routeKeys.Add(routeKey))
        {
          context.ReportDiagnostic(Diagnostic.Create(
              DuplicateRouteRule,
              invocation.GetLocation(),
              pattern,
              httpMethod));
        }
      }

      if (hasSpaFallback && !hasEmbeddedUi && spaFallbackInvocation != null)
      {
        context.ReportDiagnostic(Diagnostic.Create(
            SpaFallbackWithoutEmbeddedUiRule,
            spaFallbackInvocation.GetLocation()));
      }

      var attributePrefix = GetUiModuleAttributePathPrefix(typeSymbol, typeDeclaration, context.SemanticModel);
      if (attributePrefix != null && !hasEmbeddedUi)
      {
        context.ReportDiagnostic(Diagnostic.Create(
            MissingEmbeddedUiRule,
            typeDeclaration.GetLocation(),
            typeSymbol.Name));
      }
    }

    private static void AnalyzeMapEmbeddedUi(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        INamedTypeSymbol moduleType)
    {
      var namespaceLiteral = GetInvocationStringLiteral(invocation, context.SemanticModel, argumentIndex: 1);
      if (namespaceLiteral == null || namespaceLiteral.EndsWith(".Content", System.StringComparison.Ordinal))
        return;

      var assemblyName = moduleType.ContainingAssembly.Name;
      context.ReportDiagnostic(Diagnostic.Create(
          InvalidEmbeddedNamespaceRule,
          invocation.GetLocation(),
          namespaceLiteral,
          assemblyName));
    }

    private static void AnalyzeMapFallbackToIndex(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        HashSet<string> embeddedResources)
    {
      if (embeddedResources.Count == 0)
        return;

      var resourceName = GetInvocationStringLiteral(invocation, context.SemanticModel);
      if (resourceName == null)
        return;

      if (!embeddedResources.Contains(resourceName))
      {
        context.ReportDiagnostic(Diagnostic.Create(
            MissingFallbackResourceRule,
            invocation.GetLocation(),
            resourceName));
      }
    }

    private static HashSet<string> GetEmbeddedResourceNames(System.Collections.Immutable.ImmutableArray<AdditionalText> additionalFiles)
    {
      var resources = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
      foreach (var additionalFile in additionalFiles)
      {
        if (!additionalFile.Path.EndsWith(".dashboard.embeddedresources", System.StringComparison.OrdinalIgnoreCase))
          continue;

        foreach (var line in additionalFile.GetText()?.ToString().Split('\n') ?? System.Array.Empty<string>())
        {
          var trimmed = line.Trim();
          if (!string.IsNullOrEmpty(trimmed))
            resources.Add(trimmed);
        }
      }

      return resources;
    }

    private static bool TryGetRouteMethod(string methodName, out string httpMethod)
    {
      switch (methodName)
      {
        case "MapGet":
          httpMethod = "GET";
          return true;
        case "MapPost":
          httpMethod = "POST";
          return true;
        case "MapPut":
          httpMethod = "PUT";
          return true;
        case "MapPatch":
          httpMethod = "PATCH";
          return true;
        case "MapDelete":
          httpMethod = "DELETE";
          return true;
        case "MapOptions":
          httpMethod = "OPTIONS";
          return true;
        default:
          httpMethod = null;
          return false;
      }
    }

    private static string GetSimpleMethodName(InvocationExpressionSyntax invocation)
    {
      if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        return memberAccess.Name.Identifier.Text;

      if (invocation.Expression is IdentifierNameSyntax identifier)
        return identifier.Identifier.Text;

      return null;
    }

    private static string GetInvocationStringLiteral(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        int argumentIndex = 0)
    {
      var argumentList = invocation.ArgumentList;
      if (argumentList == null || argumentList.Arguments.Count <= argumentIndex)
        return null;

      var argument = argumentList.Arguments[argumentIndex];
      var constant = semanticModel.GetConstantValue(argument.Expression);
      return constant.HasValue ? constant.Value as string : null;
    }

    private static string GetConstantString(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel)
    {
      if (propertyDeclaration.ExpressionBody != null)
      {
        var constant = semanticModel.GetConstantValue(propertyDeclaration.ExpressionBody.Expression);
        return constant.HasValue ? constant.Value as string : null;
      }

      var getter = propertyDeclaration.AccessorList?.Accessors
          .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
      if (getter?.ExpressionBody != null)
      {
        var constant = semanticModel.GetConstantValue(getter.ExpressionBody.Expression);
        return constant.HasValue ? constant.Value as string : null;
      }

      return null;
    }

    private static bool ImplementsUiModule(INamedTypeSymbol type)
    {
      foreach (var iface in type.AllInterfaces)
      {
        if (iface.Name == "IUiModule" &&
            iface.ContainingNamespace?.ToDisplayString() == "AspNetCoreDashboard.Abstractions")
        {
          return true;
        }
      }

      return false;
    }
  }
}
