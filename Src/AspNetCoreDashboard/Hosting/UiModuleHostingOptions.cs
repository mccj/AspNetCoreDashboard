namespace AspNetCoreDashboard.Hosting
{
  /// <summary>UI 模块中间件的宿主级选项。</summary>
  public sealed class UiModuleHostingOptions
  {
    /// <summary>为模块响应应用基础安全响应头。</summary>
    public bool ApplySecurityHeaders { get; set; }

    /// <summary>启用安全响应头时可选的 Content-Security-Policy 值。</summary>
    public string? ContentSecurityPolicy { get; set; }

    /// <summary>为模块流量写入简洁的请求日志。</summary>
    public bool EnableRequestLogging { get; set; }

    /// <summary>默认最大请求体大小（字节）；0 表示不限制。</summary>
    public long MaxRequestBodyBytes { get; set; }
  }
}
