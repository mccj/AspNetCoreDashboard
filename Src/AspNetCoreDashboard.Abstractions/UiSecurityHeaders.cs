namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>UI 模块常用的安全相关响应头。</summary>
  public static class UiSecurityHeaders
  {
    /// <summary>为嵌入式管理 UI 应用推荐的基础安全响应头。</summary>
    public static void ApplyBaseline(IUiContext context, string? contentSecurityPolicy = null)
    {
      if (context == null) throw new System.ArgumentNullException(nameof(context));

      context.SetResponseHeader("X-Content-Type-Options", "nosniff");
      context.SetResponseHeader("X-Frame-Options", "SAMEORIGIN");
      context.SetResponseHeader("Referrer-Policy", "strict-origin-when-cross-origin");

      if (!string.IsNullOrEmpty(contentSecurityPolicy))
        context.SetResponseHeader("Content-Security-Policy", contentSecurityPolicy);
    }
  }
}
