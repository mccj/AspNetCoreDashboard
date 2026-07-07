using System.Reflection;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Owin.Hosting;

namespace AspNetCoreDashboard.Owin.Hosting
{
  internal sealed class OwinUiModuleRegistration : IUiModuleRegistration
  {
    private readonly OwinUiModuleDescriptor _descriptor;

    public OwinUiModuleRegistration(OwinUiModuleDescriptor descriptor)
    {
      _descriptor = descriptor;
    }

    public IUiModuleRegistration MapEmbeddedUi(Assembly assembly, string baseNamespace)
    {
      _descriptor.EmbeddedAssembly = assembly;
      _descriptor.EmbeddedBaseNamespace = baseNamespace;
      return this;
    }

    public IUiModuleRegistration MapGet(string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new UiRouteEntry { Method = UiHttpMethod.Get, Pattern = pattern, Handler = handler });
      return this;
    }

    public IUiModuleRegistration MapHead(string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new UiRouteEntry { Method = UiHttpMethod.Head, Pattern = pattern, Handler = handler });
      return this;
    }

    public IUiModuleRegistration MapPost(string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new UiRouteEntry { Method = UiHttpMethod.Post, Pattern = pattern, Handler = handler });
      return this;
    }

    public IUiModuleRegistration MapPut(string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new UiRouteEntry { Method = UiHttpMethod.Put, Pattern = pattern, Handler = handler });
      return this;
    }

    public IUiModuleRegistration MapDelete(string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new UiRouteEntry { Method = UiHttpMethod.Delete, Pattern = pattern, Handler = handler });
      return this;
    }

    public IUiModuleRegistration MapPatch(string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new UiRouteEntry { Method = UiHttpMethod.Patch, Pattern = pattern, Handler = handler });
      return this;
    }

    public IUiModuleRegistration MapOptions(string pattern, UiHandler handler)
    {
      _descriptor.Routes.Add(new UiRouteEntry { Method = UiHttpMethod.Options, Pattern = pattern, Handler = handler });
      return this;
    }

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

    public IUiModuleRegistration WithStaticCache(int maxAgeSeconds = 31536000)
    {
      _descriptor.StaticCacheMaxAgeSeconds = maxAgeSeconds;
      return this;
    }

    public IUiModuleRegistration WithContentSecurityPolicy(string contentSecurityPolicy)
    {
      _descriptor.ContentSecurityPolicy = contentSecurityPolicy;
      return this;
    }

    public IUiModuleRegistration WithMaxUploadBytes(long maxBytes)
    {
      _descriptor.MaxUploadBytes = maxBytes;
      return this;
    }

    internal void AddAuthorizationFilter(IUiAuthorizationFilter filter)
    {
      if (filter != null)
        _descriptor.AuthorizationFilters.Add(filter);
    }
  }
}
