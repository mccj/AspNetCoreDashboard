using System;
using System.Linq;
using System.Reflection;
using AspNetCoreDashboard.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>
  /// UI 模块的依赖注入扩展方法。
  /// </summary>
  public static class UiModuleServiceCollectionExtensions
  {
    /// <summary>在 DI 中注册共享的 UI 模块注册表。</summary>
    public static UiModuleRegistry AddUiModules(this IServiceCollection services)
    {
      if (services == null) throw new ArgumentNullException(nameof(services));

      if (!services.Any(descriptor => descriptor.ServiceType == typeof(IUiModuleMountTracker)))
        services.AddSingleton<IUiModuleMountTracker, UiModuleMountTracker>();

      var registry = new UiModuleRegistry();
      services.AddSingleton(registry);
      return registry;
    }

    /// <summary>在 DI 中注册模块类型，并将其添加到注册表。</summary>
    public static UiModuleRegistry AddModule<TModule>(this UiModuleRegistry registry, IServiceCollection services)
        where TModule : class, IUiModule, new()
    {
      if (registry == null) throw new ArgumentNullException(nameof(registry));
      services.AddSingleton<IUiModule, TModule>();
      return registry.AddModule(new TModule());
    }

    /// <summary>从程序集中注册所有具体的 <see cref="IUiModule"/> 类型。</summary>
    public static UiModuleRegistry AddModulesFromAssembly(this UiModuleRegistry registry, IServiceCollection services, Assembly assembly)
    {
      if (registry == null) throw new ArgumentNullException(nameof(registry));
      if (services == null) throw new ArgumentNullException(nameof(services));
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));

      foreach (var moduleType in assembly.GetUiModuleTypes())
      {
        services.AddSingleton(typeof(IUiModule), moduleType);
        registry.AddModule((IUiModule)Activator.CreateInstance(moduleType));
      }

      return registry;
    }
  }
}
