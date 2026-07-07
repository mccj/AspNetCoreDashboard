using System;
using System.IO;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// UI 模块处理程序使用的、与宿主无关的请求上下文。
  /// </summary>
  public interface IUiContext
  {
    /// <summary>获取请求作用域内的服务提供程序（若可用）。</summary>
    IServiceProvider? Services { get; }

    /// <summary>获取 HTTP 方法（例如 GET 或 POST）。</summary>
    string Method { get; }

    /// <summary>获取相对于模块路径前缀的请求路径。</summary>
    string Path { get; }

    /// <summary>获取或设置当前路由处理程序的正则匹配结果。</summary>
    Match? RouteMatch { get; set; }

    /// <summary>获取在客户端断开连接时收到通知的取消标记。</summary>
    CancellationToken RequestAborted { get; }

    /// <summary>获取已认证用户（若可用）。</summary>
    ClaimsPrincipal? User { get; }

    /// <summary>读取查询字符串参数值。</summary>
    Task<string?> GetQueryAsync(string name);

    /// <summary>读取表单字段值。</summary>
    Task<string?> GetFormValueAsync(string name);

    /// <summary>读取上传的表单文件；不存在时返回 null。</summary>
    Task<IUiFormFile?> GetFormFileAsync(string name);

    /// <summary>将整个请求体读取为字符串。</summary>
    Task<string> ReadBodyAsStringAsync();

    /// <summary>打开请求体流以供读取（用于上传与二进制负载）。</summary>
    Task<Stream> OpenRequestBodyAsync();

    /// <summary>将请求体反序列化为 JSON 对象。</summary>
    Task<T?> ReadJsonAsync<T>();

    /// <summary>获取请求头值；不存在时返回 null。</summary>
    string? GetRequestHeader(string name);

    /// <summary>获取请求 Cookie 值；不存在时返回 null。</summary>
    string? GetRequestCookie(string name);

    /// <summary>设置响应头值。</summary>
    void SetResponseHeader(string name, string value);

    /// <summary>追加 Set-Cookie 响应头。</summary>
    void SetCookie(
        string name,
        string value,
        DateTimeOffset? expires = null,
        string? path = null,
        bool httpOnly = false,
        bool secure = false,
        UiCookieSameSite sameSite = UiCookieSameSite.Unspecified);

    /// <summary>获取或设置响应的 HTTP 状态码。</summary>
    int StatusCode { get; set; }

    /// <summary>写入文本响应体。</summary>
    Task WriteAsync(string content, string contentType = "text/plain");

    /// <summary>序列化并写入 JSON 响应体。</summary>
    Task WriteJsonAsync(object value);

    /// <summary>将二进制流作为响应体写入。</summary>
    Task WriteStreamAsync(Stream stream, string contentType, string? downloadFileName = null);

    /// <summary>将客户端重定向到指定地址。</summary>
    Task RedirectAsync(string location, bool permanent = false);

    /// <summary>获取服务器的本地 IP 地址。</summary>
    string? LocalIpAddress { get; }

    /// <summary>获取客户端的远程 IP 地址。</summary>
    string? RemoteIpAddress { get; }
  }
}
