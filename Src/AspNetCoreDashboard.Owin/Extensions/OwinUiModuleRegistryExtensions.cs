using System.Linq;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Owin.Authorization;
using Owin;

namespace AspNetCoreDashboard.Owin.Extensions
{
  /// <summary>基于策略授权的 OWIN 注册表扩展方法。</summary>
  public static class OwinUiModuleRegistryExtensions
  {
    internal const string AuthorizationAdapterPropertyKey = "AspNetCoreDashboard.OwinAuthorizationAdapter";

    /// <summary>将授权适配器存储在 OWIN 应用程序生成器上。</summary>
    public static IAppBuilder UseOwinAuthorizationAdapter(this IAppBuilder app, IOwinAuthorizationAdapter adapter)
    {
      if (app == null) throw new System.ArgumentNullException(nameof(app));
      if (adapter == null) throw new System.ArgumentNullException(nameof(adapter));
      app.Properties[AuthorizationAdapterPropertyKey] = adapter;
      return app;
    }

    /// <summary>注册通过 <see cref="UseOwinAuthorizationAdapter"/> 解析的策略名称。</summary>
    public static UiModuleRegistry SetAuthorizationPolicy(
        this UiModuleRegistry registry,
        string pathPrefix,
        string policyName,
        IOwinAuthorizationAdapter adapter)
    {
      if (registry == null) throw new System.ArgumentNullException(nameof(registry));
      if (adapter == null) throw new System.ArgumentNullException(nameof(adapter));
      return registry.SetAuthorization(pathPrefix, new OwinPolicyAuthorizationFilter(policyName, adapter));
    }
  }
}
