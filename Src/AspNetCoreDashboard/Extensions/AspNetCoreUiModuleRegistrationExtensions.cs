using System;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreDashboard.Extensions
{
  /// <summary>
  /// ASP.NET Core UI 模块注册的授权扩展方法。
  /// </summary>
  public static class AspNetCoreUiModuleRegistrationExtensions
  {
    /// <summary>在模块注册期间应用授权筛选器。</summary>
    public static IUiModuleRegistration WithAuthorization(this IUiModuleRegistration registration, IUiAuthorizationFilter filter)
    {
      if (registration == null) throw new ArgumentNullException(nameof(registration));
      if (registration is AspNetCoreUiModuleRegistration aspNetRegistration)
      {
        aspNetRegistration.AddAuthorizationFilter(filter);
        return registration;
      }

      throw new InvalidOperationException("WithAuthorization(filter) 需要 ASP.NET Core 宿主包。");
    }

    /// <summary>在模块注册期间应用 ASP.NET Core 授权策略。</summary>
    public static IUiModuleRegistration WithAuthorization(this IUiModuleRegistration registration, string policyName)
    {
      if (registration == null) throw new ArgumentNullException(nameof(registration));
      if (registration is AspNetCoreUiModuleRegistration aspNetRegistration)
      {
        aspNetRegistration.SetAuthorizationPolicy(policyName);
        return registration;
      }

      throw new InvalidOperationException("WithAuthorization(policyName) 需要 ASP.NET Core 宿主包。");
    }
  }

  internal static class UiModuleDescriptorFactory
  {
    public static UiModuleDescriptor Create(IUiModule module, string authorizationPolicy = null)
    {
      var descriptor = new UiModuleDescriptor { PathPrefix = module.PathPrefix };
      var registration = new AspNetCoreUiModuleRegistration(descriptor);
      module.Configure(registration);
      if (!string.IsNullOrEmpty(authorizationPolicy))
        registration.SetAuthorizationPolicy(authorizationPolicy);
      return descriptor;
    }
  }
}
