using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Owin.Authorization;
using AspNetCoreDashboard.Owin.Extensions;
using AspNetCoreDashboard.Owin.Hosting;
using Owin;

namespace Microsoft.AspNetCore.Builder
{
  /// <summary>
  /// UI 模块的 OWIN 应用程序生成器扩展方法。
  /// </summary>
  public static class OwinUiModuleAppBuilderExtensions
  {
    internal const string RegistryPropertyKey = "AspNetCoreDashboard.UiModuleRegistry";

    /// <summary>
    /// 创建并存储在 OWIN 应用程序生成器上的模块注册表。
    /// </summary>
    public static UiModuleRegistry AddUiModules(this IAppBuilder app)
    {
      if (app == null) throw new ArgumentNullException(nameof(app));

      var registry = new UiModuleRegistry();
      app.Properties[RegistryPropertyKey] = registry;
      app.Properties[UiModuleMountTracker.OwinPropertyKey] = new UiModuleMountTracker();
      return registry;
    }

    /// <summary>
    /// 挂载通过 <see cref="AddUiModules(IAppBuilder)"/> 注册的所有模块。
    /// </summary>
    public static IAppBuilder UseUiModules(this IAppBuilder app)
    {
      if (app == null) throw new ArgumentNullException(nameof(app));

      if (!(app.Properties[RegistryPropertyKey] is UiModuleRegistry registry))
      {
        throw new InvalidOperationException(
            "未找到 UI 模块注册表。请先调用 AddUiModules()，再调用 UseUiModules()。");
      }

      return app.UseUiModules(registry);
    }

    /// <summary>
    /// 挂载给定注册表中的所有模块。
    /// </summary>
    public static IAppBuilder UseUiModules(this IAppBuilder app, UiModuleRegistry registry)
    {
      if (app == null) throw new ArgumentNullException(nameof(app));
      if (registry == null) throw new ArgumentNullException(nameof(registry));

      UiModuleStartupValidator.Validate(registry.Modules);

      var adapter = app.Properties.ContainsKey(OwinUiModuleRegistryExtensions.AuthorizationAdapterPropertyKey)
          ? app.Properties[OwinUiModuleRegistryExtensions.AuthorizationAdapterPropertyKey] as IOwinAuthorizationAdapter
          : null;

      foreach (var module in registry.Modules)
      {
        var policy = registry.GetAuthorizationPolicy(module.PathPrefix);
        var filters = registry.GetAuthorization(module.PathPrefix).ToList();
        if (!string.IsNullOrEmpty(policy))
        {
          if (adapter == null)
          {
            throw new InvalidOperationException(
                $"模块 '{module.PathPrefix}' 的 SetAuthorizationPolicy('{policy}') 需要 IOwinAuthorizationAdapter。" +
                "请在 UseUiModules() 之前调用 app.UseOwinAuthorizationAdapter(...)，或使用 SetAuthorization 配合 RequireRoleAuthorizationFilter。");
          }

          filters.Add(new OwinPolicyAuthorizationFilter(policy, adapter));
        }

        app.UseUiModule(module, filters.ToArray());
      }

      return app;
    }

    /// <summary>
    /// 挂载给定模块实例，可选配置默认授权筛选器。
    /// </summary>
    public static IAppBuilder UseUiModules(
        this IAppBuilder app,
        IEnumerable<IUiModule> modules,
        params IUiAuthorizationFilter[] defaultAuthorizationFilters)
    {
      if (app == null) throw new ArgumentNullException(nameof(app));
      if (modules == null) throw new ArgumentNullException(nameof(modules));

      foreach (var module in modules)
        app.UseUiModule(module, defaultAuthorizationFilters);

      return app;
    }

    /// <summary>按其路径前缀挂载单个 UI 模块。</summary>
    public static IAppBuilder UseUiModule(
        this IAppBuilder app,
        IUiModule module,
        params IUiAuthorizationFilter[] authorizationFilters)
    {
      if (app == null) throw new ArgumentNullException(nameof(app));
      if (module == null) throw new ArgumentNullException(nameof(module));

      var tracker = app.Properties.ContainsKey(UiModuleMountTracker.OwinPropertyKey)
          ? app.Properties[UiModuleMountTracker.OwinPropertyKey] as IUiModuleMountTracker
          : null;
      if (tracker == null)
      {
        tracker = new UiModuleMountTracker();
        app.Properties[UiModuleMountTracker.OwinPropertyKey] = tracker;
      }
      tracker.Register(module.PathPrefix);

      var descriptor = OwinUiModuleDescriptorFactory.Create(module, authorizationFilters);
      var path = NormalizePath(module.PathPrefix);

      app.Map(path, branch =>
      {
        branch.Use<OwinUiModulePipelineMiddleware>(descriptor);
        branch.Use<OwinUiModuleMiddleware>(descriptor);
      });

      return app;
    }

    /// <summary>按类型挂载单个 UI 模块。</summary>
    public static IAppBuilder UseUiModule<TModule>(
        this IAppBuilder app,
        params IUiAuthorizationFilter[] authorizationFilters)
        where TModule : IUiModule, new()
    {
      return app.UseUiModule(new TModule(), authorizationFilters);
    }

    private static string NormalizePath(string pathPrefix)
    {
      if (string.IsNullOrWhiteSpace(pathPrefix))
        return "/";
      return pathPrefix.TrimEnd('/');
    }
  }
}
