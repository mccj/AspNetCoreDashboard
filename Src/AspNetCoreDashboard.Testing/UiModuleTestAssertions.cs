using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace AspNetCoreDashboard.Testing
{
  /// <summary>UI 模块集成测试的断言辅助方法。</summary>
  public static class UiModuleTestAssertions
  {
    /// <summary>断言响应包含 ETag 头。</summary>
    public static void AssertHasEtag(HttpResponseMessage response)
    {
      if (response?.Headers.ETag == null || string.IsNullOrEmpty(response.Headers.ETag.Tag))
        throw new InvalidOperationException("期望响应包含 ETag 头。");
    }

    /// <summary>断言返回 304 Not Modified 响应。</summary>
    public static void AssertNotModified(HttpResponseMessage response)
    {
      if (response == null || response.StatusCode != HttpStatusCode.NotModified)
        throw new InvalidOperationException("期望 HTTP 304 Not Modified。");
    }

    /// <summary>断言存在基线安全响应头。</summary>
    public static void AssertSecurityHeaders(HttpResponseMessage response)
    {
      if (response == null)
        throw new ArgumentNullException(nameof(response));

      var nosniff = response.Headers.TryGetValues("X-Content-Type-Options", out var values)
          ? values.FirstOrDefault()
          : null;
      if (!string.Equals(nosniff, "nosniff", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("期望 X-Content-Type-Options: nosniff。");
    }

    /// <summary>断言未授权（401）。</summary>
    public static void AssertUnauthorized(HttpResponseMessage response)
    {
      if (response == null || response.StatusCode != HttpStatusCode.Unauthorized)
        throw new InvalidOperationException("期望 HTTP 401 Unauthorized。");
    }

    /// <summary>断言禁止访问（403）。</summary>
    public static void AssertForbidden(HttpResponseMessage response)
    {
      if (response == null || response.StatusCode != HttpStatusCode.Forbidden)
        throw new InvalidOperationException("期望 HTTP 403 Forbidden。");
    }
  }
}
