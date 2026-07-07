using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// 当客户端 IP（来自 <c>X-Forwarded-For</c>）为回环地址或与受信任代理跳数匹配时允许请求。
  /// 生产环境中请配置受信任的代理地址；切勿信任任意 <c>X-Forwarded-For</c> 值。
  /// </summary>
  public sealed class TrustedForwardedHeadersAuthorizationFilter : IUiAuthorizationFilter
  {
    private readonly HashSet<string> _trustedProxies;

    /// <summary>创建信任回环地址及可选代理 IP 的筛选器。</summary>
    public TrustedForwardedHeadersAuthorizationFilter(params string[] trustedProxyAddresses)
    {
      _trustedProxies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "127.0.0.1",
                "::1"
            };

      if (trustedProxyAddresses != null)
      {
        foreach (var proxy in trustedProxyAddresses)
        {
          if (!string.IsNullOrWhiteSpace(proxy))
            _trustedProxies.Add(proxy);
        }
      }
    }

    /// <inheritdoc />
    public bool Authorize(IUiContext context)
    {
      if (context == null) throw new ArgumentNullException(nameof(context));

      var clientIp = ResolveClientIp(context);
      if (string.IsNullOrEmpty(clientIp))
        return false;

      if (clientIp == "127.0.0.1" || clientIp == "::1")
        return true;

      if (clientIp == context.LocalIpAddress)
        return true;

      return false;
    }

    private string? ResolveClientIp(IUiContext context)
    {
      var forwardedFor = context.GetRequestHeader("X-Forwarded-For");
      if (!string.IsNullOrEmpty(forwardedFor))
      {
        var hops = forwardedFor.Split(',')
            .Select(h => h.Trim())
            .Where(h => h.Length > 0)
            .ToArray();

        if (hops.Length > 0)
        {
          var remote = context.RemoteIpAddress;
          if (!string.IsNullOrEmpty(remote) && _trustedProxies.Contains(remote))
            return hops[0];

          return null;
        }
      }

      return context.RemoteIpAddress;
    }
  }
}
