using System;
using AspNetCoreDashboard.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>UI 模块的宿主选项与健康检查扩展。</summary>
  public static class UiModuleHostingServiceCollectionExtensions
  {
    /// <summary>配置全局 UI 模块宿主选项。</summary>
    public static IServiceCollection AddUiModuleHosting(
        this IServiceCollection services,
        Action<UiModuleHostingOptions> configure)
    {
      if (services == null) throw new ArgumentNullException(nameof(services));
      if (configure == null) throw new ArgumentNullException(nameof(configure));

      services.Configure(configure);
      return services;
    }

    /// <summary>注册用于列出已挂载 UI 模块的健康检查。</summary>
    public static IHealthChecksBuilder AddUiModulesHealthCheck(this IHealthChecksBuilder builder)
    {
      if (builder == null) throw new ArgumentNullException(nameof(builder));

      return builder.AddCheck<UiModulesHealthCheck>("ui_modules");
    }
  }
}
