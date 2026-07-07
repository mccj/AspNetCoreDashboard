using System;
using System.Linq;
using System.Reflection;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>UI 模块的程序集扫描辅助方法。</summary>
  public static class UiModuleAssemblyExtensions
  {
    /// <summary>返回程序集中所有具体 <see cref="IUiModule"/> 类型。</summary>
    public static Type[] GetUiModuleTypes(this Assembly assembly)
    {
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));

      return assembly.GetTypes()
          .Where(type => typeof(IUiModule).IsAssignableFrom(type)
              && type.IsClass
              && !type.IsAbstract
              && type.GetConstructor(Type.EmptyTypes) != null)
          .OrderBy(type => type.FullName, StringComparer.Ordinal)
          .ToArray();
    }
  }
}
