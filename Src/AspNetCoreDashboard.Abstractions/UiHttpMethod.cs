namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>UI 模块路由处理程序支持的 HTTP 方法。</summary>
  public enum UiHttpMethod
  {
    /// <summary>HTTP GET。</summary>
    Get,

    /// <summary>HTTP HEAD（无显式 HEAD 路由时回退到 GET 路由处理程序）。</summary>
    Head,

    /// <summary>HTTP POST。</summary>
    Post,

    /// <summary>HTTP PUT。</summary>
    Put,

    /// <summary>HTTP PATCH。</summary>
    Patch,

    /// <summary>HTTP DELETE。</summary>
    Delete,

    /// <summary>HTTP OPTIONS。</summary>
    Options
  }
}
