using System;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Owin.Hosting;
using Owin;

namespace AspNetCoreDashboard.Owin.Extensions
{
  /// <summary>
  /// UI 模块注册的 OWIN 授权扩展方法。
  /// </summary>
  public static class OwinUiModuleRegistrationExtensions
  {
    /// <summary>在模块注册期间应用授权筛选器。</summary>
    public static IUiModuleRegistration WithAuthorization(this IUiModuleRegistration registration, IUiAuthorizationFilter filter)
    {
      if (registration == null) throw new ArgumentNullException(nameof(registration));
      if (registration is OwinUiModuleRegistration owinRegistration)
      {
        owinRegistration.AddAuthorizationFilter(filter);
        return registration;
      }

      throw new InvalidOperationException("WithAuthorization(filter) 需要 OWIN 宿主包。");
    }

    /// <summary>在模块注册期间要求已认证用户至少属于指定角色之一。</summary>
    public static IUiModuleRegistration WithRequiredRole(this IUiModuleRegistration registration, params string[] roles)
    {
      if (registration == null) throw new ArgumentNullException(nameof(registration));
      return registration.WithAuthorization(new RequireRoleAuthorizationFilter(roles));
    }
  }

  internal static class OwinUiModuleDescriptorFactory
  {
    public static OwinUiModuleDescriptor Create(IUiModule module, params IUiAuthorizationFilter[] filters)
    {
      var descriptor = new OwinUiModuleDescriptor { PathPrefix = module.PathPrefix };
      var registration = new OwinUiModuleRegistration(descriptor);
      module.Configure(registration);

      if (filters == null || filters.Length == 0)
        return descriptor;

      var merged = new System.Collections.Generic.List<IUiAuthorizationFilter>(filters);
      merged.AddRange(descriptor.AuthorizationFilters);
      descriptor.AuthorizationFilters.Clear();
      foreach (var filter in merged)
        descriptor.AuthorizationFilters.Add(filter);

      return descriptor;
    }
  }
}
