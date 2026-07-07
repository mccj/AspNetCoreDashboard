using System.Reflection;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// 由各宿主包实现的流式注册 API。
  /// </summary>
  public interface IUiModuleRegistration
  {
    /// <summary>从指定程序集命名空间映射嵌入式静态文件。</summary>
    IUiModuleRegistration MapEmbeddedUi(Assembly assembly, string baseNamespace);

    /// <summary>注册相对于模块路径前缀的 GET 路由处理程序。</summary>
    IUiModuleRegistration MapGet(string pattern, UiHandler handler);

    /// <summary>注册相对于模块路径前缀的 HEAD 路由处理程序。</summary>
    IUiModuleRegistration MapHead(string pattern, UiHandler handler);

    /// <summary>注册相对于模块路径前缀的 POST 路由处理程序。</summary>
    IUiModuleRegistration MapPost(string pattern, UiHandler handler);

    /// <summary>注册相对于模块路径前缀的 PUT 路由处理程序。</summary>
    IUiModuleRegistration MapPut(string pattern, UiHandler handler);

    /// <summary>注册相对于模块路径前缀的 DELETE 路由处理程序。</summary>
    IUiModuleRegistration MapDelete(string pattern, UiHandler handler);

    /// <summary>注册相对于模块路径前缀的 PATCH 路由处理程序。</summary>
    IUiModuleRegistration MapPatch(string pattern, UiHandler handler);

    /// <summary>注册相对于模块路径前缀的 OPTIONS 路由处理程序。</summary>
    IUiModuleRegistration MapOptions(string pattern, UiHandler handler);

    /// <summary>为未匹配的路由注册回退 index.html 资源。</summary>
    IUiModuleRegistration MapFallbackToIndex(string indexResourceName);

    /// <summary>注册 SPA History 模式回退（与 <see cref="MapFallbackToIndex"/> 相同）。</summary>
    IUiModuleRegistration MapSpaFallback(string indexResourceName);

    /// <summary>将指定文件扩展名排除在 SPA 回退之外（例如 <c>.json</c>、<c>.map</c>）。</summary>
    IUiModuleRegistration ExcludeSpaFallbackExtensions(params string[] extensions);

    /// <summary>
    /// 为静态资源设置长期缓存（秒）。index.html 始终使用 no-cache 提供。
    /// </summary>
    IUiModuleRegistration WithStaticCache(int maxAgeSeconds = 31536000);

    /// <summary>设置模块专用的 Content-Security-Policy 响应头。</summary>
    IUiModuleRegistration WithContentSecurityPolicy(string contentSecurityPolicy);

    /// <summary>拒绝本模块中超过指定字节限制的请求体。</summary>
    IUiModuleRegistration WithMaxUploadBytes(long maxBytes);
  }
}
