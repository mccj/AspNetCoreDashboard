using System;
using System.Reflection;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>已注册 UI 模块的运行时诊断信息。</summary>
  public sealed class UiModuleSummary
  {
    /// <summary>模块挂载路径。</summary>
    public string PathPrefix { get; internal set; }

    /// <summary>模块 CLR 类型名称。</summary>
    public string ModuleType { get; internal set; }

    /// <summary>在 Configure() 中注册的 API 路由数量。</summary>
    public int RouteCount { get; internal set; }

    /// <summary>是否调用了 MapEmbeddedUi。</summary>
    public bool HasEmbeddedUi { get; internal set; }

    /// <summary>已配置的回退 index 资源名称。</summary>
    public string FallbackIndexResource { get; internal set; }
  }

  /// <summary>在不执行处理程序的情况下检查模块配置。</summary>
  public static class UiModuleDiagnostics
  {
    /// <summary>通过对探测注册对象运行 Configure() 来构建摘要信息。</summary>
    public static UiModuleSummary GetSummary(IUiModule module)
    {
      if (module == null) throw new ArgumentNullException(nameof(module));

      var probe = new ProbeRegistration();
      module.Configure(probe);
      return new UiModuleSummary
      {
        PathPrefix = module.PathPrefix,
        ModuleType = module.GetType().Name,
        RouteCount = probe.RouteCount,
        HasEmbeddedUi = probe.HasEmbeddedUi,
        FallbackIndexResource = probe.FallbackIndexResource
      };
    }

    private sealed class ProbeRegistration : IUiModuleRegistration
    {
      public int RouteCount { get; private set; }
      public bool HasEmbeddedUi { get; private set; }
      public string FallbackIndexResource { get; private set; }

      public IUiModuleRegistration MapEmbeddedUi(Assembly assembly, string baseNamespace)
      {
        HasEmbeddedUi = true;
        return this;
      }

      public IUiModuleRegistration MapGet(string pattern, UiHandler handler) { RouteCount++; return this; }
      public IUiModuleRegistration MapHead(string pattern, UiHandler handler) { RouteCount++; return this; }
      public IUiModuleRegistration MapPost(string pattern, UiHandler handler) { RouteCount++; return this; }
      public IUiModuleRegistration MapPut(string pattern, UiHandler handler) { RouteCount++; return this; }
      public IUiModuleRegistration MapDelete(string pattern, UiHandler handler) { RouteCount++; return this; }
      public IUiModuleRegistration MapPatch(string pattern, UiHandler handler) { RouteCount++; return this; }
      public IUiModuleRegistration MapOptions(string pattern, UiHandler handler) { RouteCount++; return this; }

      public IUiModuleRegistration MapFallbackToIndex(string indexResourceName)
      {
        FallbackIndexResource = indexResourceName;
        return this;
      }

      public IUiModuleRegistration MapSpaFallback(string indexResourceName) => MapFallbackToIndex(indexResourceName);

      public IUiModuleRegistration ExcludeSpaFallbackExtensions(params string[] extensions) => this;

      public IUiModuleRegistration WithStaticCache(int maxAgeSeconds = 31536000) => this;

      public IUiModuleRegistration WithContentSecurityPolicy(string contentSecurityPolicy) => this;

      public IUiModuleRegistration WithMaxUploadBytes(long maxBytes) => this;
    }
  }
}
