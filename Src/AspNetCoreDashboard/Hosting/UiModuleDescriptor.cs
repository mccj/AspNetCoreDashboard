using System.Collections.Generic;
using System.Reflection;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Hosting
{
  internal sealed class UiRouteEntry
  {
    public UiHttpMethod Method { get; set; }
    public string Pattern { get; set; }
    public UiHandler Handler { get; set; }
  }

  internal sealed class UiModuleDescriptor
  {
    public string PathPrefix { get; set; }
    public Assembly EmbeddedAssembly { get; set; }
    public string EmbeddedBaseNamespace { get; set; }
    public string FallbackIndexResource { get; set; }
    public IList<string> SpaFallbackExcludedExtensions { get; } = new List<string>();
    public string AuthorizationPolicy { get; set; }
    public string ContentSecurityPolicy { get; set; }
    public long MaxUploadBytes { get; set; }
    public int StaticCacheMaxAgeSeconds { get; set; } = 31536000;
    public IList<IUiAuthorizationFilter> AuthorizationFilters { get; } = new List<IUiAuthorizationFilter>();
    public IList<UiRouteEntry> Routes { get; } = new List<UiRouteEntry>();

    public bool HasEmbeddedUi => EmbeddedAssembly != null && !string.IsNullOrEmpty(EmbeddedBaseNamespace);
  }
}
