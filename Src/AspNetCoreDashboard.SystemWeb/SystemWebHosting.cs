using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.SystemWeb
{
  /// <summary>System.Web UI 模块注册表。</summary>
  public static class UiModuleHttpModuleRegistry
  {
    private static readonly List<SystemWebUiModuleDescriptor> Modules = new List<SystemWebUiModuleDescriptor>();

    /// <summary>为 System.Web 托管注册模块。</summary>
    public static void Register(IUiModule module)
    {
      if (module == null) throw new ArgumentNullException(nameof(module));

      var descriptor = new SystemWebUiModuleDescriptor { PathPrefix = module.PathPrefix.TrimEnd('/') };
      var registration = new SystemWebUiModuleRegistration(descriptor);
      module.Configure(registration);
      descriptor.RouteTable = BuildRouteTable(descriptor);
      Modules.Add(descriptor);
    }

    /// <summary>验证并注册多个模块。</summary>
    public static void RegisterAll(IEnumerable<IUiModule> modules)
    {
      if (modules == null) throw new ArgumentNullException(nameof(modules));
      var list = modules.ToList();
      UiModuleStartupValidator.Validate(list);
      foreach (var module in list)
        Register(module);
    }

    internal static bool TryServe(HttpContext context)
    {
      var path = context.Request.Path ?? "/";
      foreach (var module in Modules)
      {
        if (!path.StartsWith(module.PathPrefix, StringComparison.OrdinalIgnoreCase))
          continue;

        context.Items["SystemWebUiModulePathPrefix"] = module.PathPrefix;
        var relative = path.Substring(module.PathPrefix.Length).TrimStart('/');
        if (string.IsNullOrEmpty(relative))
          relative = "/";

        var uiContext = new SystemWebUiContext(context);
        foreach (var filter in module.AuthorizationFilters)
        {
          if (!filter.Authorize(uiContext))
          {
            var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
            context.Response.StatusCode = isAuthenticated ? 403 : 401;
            context.Response.End();
            return true;
          }
        }

        if (module.RouteTable != null &&
            module.RouteTable.TryMatch(context.Request.HttpMethod, relative, out var match, out var handler))
        {
          uiContext.RouteMatch = match;
          RunHandler(handler, uiContext);
          context.Response.End();
          return true;
        }

        if (TryServeStatic(context, module, relative))
          return true;

        if (SpaFallbackHelper.ShouldFallback(relative, module.FallbackIndexResource, module.SpaFallbackExcludedExtensions) &&
            module.EmbeddedAssembly != null)
        {
          var fallbackResource = module.FallbackIndexResource;
          if (string.IsNullOrEmpty(fallbackResource))
            return false;

          WriteResource(context, module.EmbeddedAssembly, fallbackResource!, "text/html");
          context.Response.End();
          return true;
        }

        return false;
      }

      return false;
    }

    internal static void Reset()
    {
      Modules.Clear();
    }

    private static UiRouteTable BuildRouteTable(SystemWebUiModuleDescriptor descriptor)
    {
      var table = new UiRouteTable();
      foreach (var route in descriptor.Routes)
        table.Add(route.Method, route.Pattern, route.Handler);
      return table;
    }

    private static void RunHandler(UiHandler handler, SystemWebUiContext context)
    {
      handler(context).GetAwaiter().GetResult();
    }

    private static bool TryServeStatic(HttpContext context, SystemWebUiModuleDescriptor module, string relative)
    {
      if (!module.HasEmbeddedUi)
        return false;

      var relativePath = relative.TrimStart('/');
      if (string.IsNullOrEmpty(relativePath))
        relativePath = "index.html";

      var assembly = module.EmbeddedAssembly;
      var baseNamespace = module.EmbeddedBaseNamespace;
      if (assembly == null || string.IsNullOrEmpty(baseNamespace))
        return false;

      var resourceName = EmbeddedResourcePathHelper.ToResourceName(baseNamespace!, relativePath);
      using (var stream = EmbeddedResourceCache.OpenReadStream(assembly, resourceName))
      {
        if (stream == null)
          return false;

        context.Response.ContentType = GuessContentType(relativePath);
        context.Response.Cache.SetCacheability(relativePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            ? HttpCacheability.NoCache
            : HttpCacheability.Public);
        stream.CopyTo(context.Response.OutputStream);
        context.Response.End();
        return true;
      }
    }

    private static void WriteResource(HttpContext context, Assembly assembly, string resourceName, string contentType)
    {
      if (assembly == null || string.IsNullOrEmpty(resourceName))
      {
        context.Response.StatusCode = 404;
        return;
      }

      using (var stream = EmbeddedResourceCache.OpenReadStream(assembly, resourceName))
      {
        if (stream == null)
        {
          context.Response.StatusCode = 404;
          return;
        }

        context.Response.ContentType = contentType;
        stream.CopyTo(context.Response.OutputStream);
      }
    }

    private static string GuessContentType(string relativePath)
    {
      var extension = Path.GetExtension(relativePath);
      switch (extension?.ToLowerInvariant())
      {
        case ".html": return "text/html";
        case ".css": return "text/css";
        case ".js": return "application/javascript";
        case ".png": return "image/png";
        default: return "application/octet-stream";
      }
    }
  }

  internal static class SpaFallbackHelper
  {
    public static bool ShouldFallback(string path, string fallbackResource, IList<string> excludedExtensions)
    {
      if (string.IsNullOrEmpty(fallbackResource))
        return false;

      if (path == "/" || string.IsNullOrEmpty(path))
        return true;

      if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        return false;

      var extension = Path.GetExtension(path);
      if (!string.IsNullOrEmpty(extension))
      {
        foreach (var excluded in excludedExtensions)
        {
          if (string.Equals(extension, excluded, StringComparison.OrdinalIgnoreCase))
            return false;
        }
      }

      return true;
    }
  }

  internal sealed class SystemWebUiModuleDescriptor
  {
    public string PathPrefix { get; set; } = "/";
    public Assembly EmbeddedAssembly { get; set; }
    public string EmbeddedBaseNamespace { get; set; }
    public string FallbackIndexResource { get; set; }
    public IList<string> SpaFallbackExcludedExtensions { get; } = new List<string>();
    public IList<IUiAuthorizationFilter> AuthorizationFilters { get; } = new List<IUiAuthorizationFilter>();
    public IList<SystemWebRouteEntry> Routes { get; } = new List<SystemWebRouteEntry>();
    public UiRouteTable RouteTable { get; set; }
    public bool HasEmbeddedUi => EmbeddedAssembly != null && !string.IsNullOrEmpty(EmbeddedBaseNamespace);
  }

  internal sealed class SystemWebRouteEntry
  {
    public UiHttpMethod Method { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public UiHandler Handler { get; set; } = default!;
  }

  internal sealed class SystemWebUiModuleRegistration : IUiModuleRegistration
  {
    private readonly SystemWebUiModuleDescriptor _descriptor;

    public SystemWebUiModuleRegistration(SystemWebUiModuleDescriptor descriptor)
    {
      _descriptor = descriptor;
    }

    public IUiModuleRegistration MapEmbeddedUi(Assembly assembly, string baseNamespace)
    {
      _descriptor.EmbeddedAssembly = assembly;
      _descriptor.EmbeddedBaseNamespace = baseNamespace;
      return this;
    }

    public IUiModuleRegistration MapGet(string pattern, UiHandler handler) => MapRoute(UiHttpMethod.Get, pattern, handler);
    public IUiModuleRegistration MapHead(string pattern, UiHandler handler) => MapRoute(UiHttpMethod.Head, pattern, handler);
    public IUiModuleRegistration MapPost(string pattern, UiHandler handler) => MapRoute(UiHttpMethod.Post, pattern, handler);
    public IUiModuleRegistration MapPut(string pattern, UiHandler handler) => MapRoute(UiHttpMethod.Put, pattern, handler);
    public IUiModuleRegistration MapDelete(string pattern, UiHandler handler) => MapRoute(UiHttpMethod.Delete, pattern, handler);
    public IUiModuleRegistration MapPatch(string pattern, UiHandler handler) => MapRoute(UiHttpMethod.Patch, pattern, handler);
    public IUiModuleRegistration MapOptions(string pattern, UiHandler handler) => MapRoute(UiHttpMethod.Options, pattern, handler);

    public IUiModuleRegistration MapFallbackToIndex(string indexResourceName)
    {
      _descriptor.FallbackIndexResource = indexResourceName;
      return this;
    }

    public IUiModuleRegistration MapSpaFallback(string indexResourceName) => MapFallbackToIndex(indexResourceName);

    public IUiModuleRegistration ExcludeSpaFallbackExtensions(params string[] extensions)
    {
      if (extensions != null)
      {
        foreach (var extension in extensions)
        {
          if (!string.IsNullOrWhiteSpace(extension))
            _descriptor.SpaFallbackExcludedExtensions.Add(extension);
        }
      }

      return this;
    }

    public IUiModuleRegistration WithStaticCache(int maxAgeSeconds = 31536000) => this;

    public IUiModuleRegistration WithContentSecurityPolicy(string contentSecurityPolicy) => this;

    public IUiModuleRegistration WithMaxUploadBytes(long maxBytes) => this;

    internal void AddAuthorizationFilter(IUiAuthorizationFilter filter)
    {
      _descriptor.AuthorizationFilters.Add(filter);
    }

    private IUiModuleRegistration MapRoute(UiHttpMethod method, string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new SystemWebRouteEntry
      {
        Method = method,
        Pattern = pattern,
        Handler = handler
      });
      return this;
    }
  }
}
