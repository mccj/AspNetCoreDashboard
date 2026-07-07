using System;
using System.Collections.Generic;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>在应用程序启动时验证已注册的 UI 模块。</summary>
  public static class UiModuleStartupValidator
  {
    /// <summary>当模块配置无效或存在冲突时抛出异常。</summary>
    public static void Validate(IReadOnlyList<IUiModule> modules)
    {
      if (modules == null) throw new ArgumentNullException(nameof(modules));

      var prefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var module in modules)
      {
        if (module == null)
          throw new InvalidOperationException("已注册了一个 null 模块实例。");

        var prefix = module.PathPrefix?.TrimEnd('/') ?? string.Empty;
        if (string.IsNullOrWhiteSpace(prefix) || !prefix.StartsWith("/", StringComparison.Ordinal))
        {
          throw new InvalidOperationException(
              $"模块 '{module.GetType().FullName}' 的 PathPrefix '{module.PathPrefix}' 无效。PathPrefix 必须以 '/' 开头。");
        }

        if (!prefixes.Add(prefix))
        {
          throw new InvalidOperationException(
              $"启动时检测到重复的 UI 模块 PathPrefix '{prefix}'。");
        }
      }
    }
  }
}
