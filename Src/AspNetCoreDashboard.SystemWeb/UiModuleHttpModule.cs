using System;
using System.Web;

namespace AspNetCoreDashboard.SystemWeb
{
  /// <summary>
  /// 用于 System.Web 的 HttpModule，提供嵌入式静态 UI 并为已注册模块执行 API 路由。
  /// </summary>
  public sealed class UiModuleHttpModule : IHttpModule
  {
    /// <inheritdoc />
    public void Init(HttpApplication context)
    {
      if (context == null) throw new ArgumentNullException(nameof(context));
      context.BeginRequest += (_, __) =>
      {
        if (HttpContext.Current != null)
          UiModuleHttpModuleRegistry.TryServe(HttpContext.Current);
      };
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
  }
}
