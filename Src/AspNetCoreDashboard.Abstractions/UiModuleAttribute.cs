using System;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>声明 UI 模块类的元数据，供分析器与源代码生成器使用。</summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public sealed class UiModuleAttribute : Attribute
  {
    /// <summary>创建模块元数据。</summary>
    public UiModuleAttribute(string pathPrefix)
    {
      if (string.IsNullOrWhiteSpace(pathPrefix))
        throw new ArgumentException("必须提供 PathPrefix。", nameof(pathPrefix));

      PathPrefix = pathPrefix;
    }

    /// <summary>模块挂载路径（必须以 <c>/</c> 开头）。</summary>
    public string PathPrefix { get; }

    /// <summary>嵌入式内容根命名空间。省略时默认为 <c>{ClassName}.Content</c>。</summary>
    public string ContentNamespace { get; set; }
  }
}
