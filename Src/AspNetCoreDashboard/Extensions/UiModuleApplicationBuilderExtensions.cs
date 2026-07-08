using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Authorization;
using AspNetCoreDashboard.Extensions;
using AspNetCoreDashboard.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
  /// <summary>
  /// UI 模块的应用程序生成器扩展方法。
  /// </summary>
  public static class UiModuleApplicationBuilderExtensions
  {
    /// <summary>挂载已在 DI 中注册的所有模块。</summary>
    public static IApplicationBuilder UseUiModules(this IApplicationBuilder app)
    {
      if (app == null) throw new ArgumentNullException(nameof(app));

      var modules = app.ApplicationServices.GetServices<IUiModule>().ToList();
      var registry = app.ApplicationServices.GetService<UiModuleRegistry>();

      UiModuleStartupValidator.Validate(modules);

      foreach (var module in modules)
      {
        var filters = registry?.GetAuthorization(module.PathPrefix) ?? Array.Empty<IUiAuthorizationFilter>();
        var policy = registry?.GetAuthorizationPolicy(module.PathPrefix);
        app.UseUiModule(module, policy, filters);
      }

      return app;
    }

    /// <summary>挂载单个 UI 模块，可选 ASP.NET Core 授权策略。</summary>
    public static IApplicationBuilder UseUiModule(
        this IApplicationBuilder app,
        IUiModule module,
        string authorizationPolicy,
        params IUiAuthorizationFilter[] authorizationFilters)
    {
      if (app == null) throw new ArgumentNullException(nameof(app));
      if (module == null) throw new ArgumentNullException(nameof(module));

      var tracker = app.ApplicationServices.GetService<IUiModuleMountTracker>() ?? new UiModuleMountTracker();
      tracker.Register(module.PathPrefix);

      var descriptor = UiModuleDescriptorFactory.Create(module, authorizationPolicy);
      var filters = BuildAuthorizationFilters(descriptor, authorizationFilters);

      app.Map(new PathString(NormalizePath(module.PathPrefix)), branch =>
      {
        AspNetCoreUiModuleBranchBuilder.ConfigureBranch(branch, descriptor, filters);
      });

      return app;
    }

    /// <summary>按其路径前缀挂载单个 UI 模块。</summary>
    public static IApplicationBuilder UseUiModule(
        this IApplicationBuilder app,
        IUiModule module,
        params IUiAuthorizationFilter[] authorizationFilters)
    {
      return app.UseUiModule(module, authorizationPolicy: null, authorizationFilters);
    }

    /// <summary>按类型挂载单个 UI 模块。</summary>
    public static IApplicationBuilder UseUiModule<TModule>(
        this IApplicationBuilder app,
        params IUiAuthorizationFilter[] authorizationFilters)
        where TModule : IUiModule, new()
    {
      return app.UseUiModule(new TModule(), authorizationFilters);
    }

    private static IUiAuthorizationFilter[] BuildAuthorizationFilters(
        UiModuleDescriptor descriptor,
        IUiAuthorizationFilter[] authorizationFilters)
    {
      var filters = new List<IUiAuthorizationFilter>();
      if (authorizationFilters != null)
        filters.AddRange(authorizationFilters);
      filters.AddRange(descriptor.AuthorizationFilters);

      if (!string.IsNullOrEmpty(descriptor.AuthorizationPolicy))
        filters.Add(new PolicyUiAuthorizationFilter(descriptor.AuthorizationPolicy));

      return filters.ToArray();
    }

    private static string NormalizePath(string pathPrefix)
    {
      if (string.IsNullOrWhiteSpace(pathPrefix))
        return "/";
      return pathPrefix.TrimEnd('/');
    }
  }
}
