using System;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// 仅允许本地请求（127.0.0.1、::1 或与本地 IP 匹配）。
  /// </summary>
  public sealed class LocalRequestsOnlyAuthorizationFilter : IUiAuthorizationFilter
  {
    /// <inheritdoc />
    public bool Authorize(IUiContext context)
    {
      if (context == null) throw new ArgumentNullException(nameof(context));

      if (string.IsNullOrEmpty(context.RemoteIpAddress))
        return false;

      if (context.RemoteIpAddress == "127.0.0.1" || context.RemoteIpAddress == "::1")
        return true;

      if (context.RemoteIpAddress == context.LocalIpAddress)
        return true;

      return false;
    }
  }
}
