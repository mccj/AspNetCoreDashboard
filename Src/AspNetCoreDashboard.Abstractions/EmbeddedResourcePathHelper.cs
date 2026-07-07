using System;
using System.IO;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// 将 URL 路径映射为嵌入式资源名称。
  /// </summary>
  public static class EmbeddedResourcePathHelper
  {
    /// <summary>
    /// 将 URL 路径转换为给定基础命名空间下的清单资源名称。
    /// </summary>
    /// <param name="baseNamespace">嵌入式资源根命名空间（例如 MyModule.Content）。</param>
    /// <param name="path">相对 URL 路径（例如 css/site.css）。</param>
    public static string ToResourceName(string baseNamespace, string path)
    {
      if (string.IsNullOrWhiteSpace(path) || path == "/")
        return baseNamespace;

      path = path.TrimStart('/');
      var fileName = Path.GetFileName(path);
      var directoryName = Path.GetDirectoryName(path)?.Replace('\\', '/');

      if (string.IsNullOrWhiteSpace(directoryName))
        return baseNamespace + "." + fileName;

      var directoryPart = directoryName
          .Replace("/", ".")
          .Replace("-", "_");

      return baseNamespace + "." + directoryPart + "." + fileName;
    }
  }
}
