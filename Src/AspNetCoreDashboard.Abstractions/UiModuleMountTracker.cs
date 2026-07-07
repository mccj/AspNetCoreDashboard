using System;
using System.Collections.Generic;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>默认的内存挂载跟踪器。</summary>
  public sealed class UiModuleMountTracker : IUiModuleMountTracker
  {
    /// <summary>OWIN <c>IAppBuilder.Properties</c> 中存放挂载跟踪器实例的键。</summary>
    public const string OwinPropertyKey = "AspNetCoreDashboard.UiModuleMountTracker";

    private readonly HashSet<string> _mountedPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new object();

    /// <inheritdoc />
    public void Register(string pathPrefix)
    {
      var normalized = Normalize(pathPrefix);
      lock (_sync)
      {
        if (!_mountedPrefixes.Add(normalized))
        {
          throw new InvalidOperationException(
              $"PathPrefix 为 '{pathPrefix}' 的 UI 模块已挂载。");
        }
      }
    }

    /// <inheritdoc />
    public void Reset()
    {
      lock (_sync)
        _mountedPrefixes.Clear();
    }

    internal static string Normalize(string pathPrefix)
    {
      if (string.IsNullOrWhiteSpace(pathPrefix))
        return "/";
      return pathPrefix.TrimEnd('/');
    }
  }
}
