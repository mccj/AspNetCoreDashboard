using System;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreDashboard.Testing
{
  /// <summary>
  /// 创建内存中的 ASP.NET Core 宿主，并配置 UI 模块以供集成测试使用。
  /// 测试现有宿主入口点时，请优先直接使用 <see cref="WebApplicationFactory{TEntryPoint}"/>。
  /// </summary>
  public sealed class UiModuleWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
      where TEntryPoint : class
  {
    private readonly Action<UiModuleRegistry, IServiceCollection> _configureModules;

    /// <summary>
    /// 创建工厂。当提供 <paramref name="configureModules"/> 时，它会在宿主启动期间运行，
    /// 以注册模块（如需要，在回调中调用 <c>services.AddUiModules()</c>）。
    /// </summary>
    public UiModuleWebApplicationFactory(Action<UiModuleRegistry, IServiceCollection> configureModules = null)
    {
      _configureModules = configureModules;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      if (_configureModules == null)
        return;

      builder.UseEnvironment("Development");
      builder.ConfigureServices(services =>
      {
        var registry = services.AddUiModules();
        _configureModules(registry, services);
      });

      builder.Configure(app => app.UseUiModules());
    }
  }
}
