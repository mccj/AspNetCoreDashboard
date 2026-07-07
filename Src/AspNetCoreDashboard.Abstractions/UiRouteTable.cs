using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>ASP.NET Core 与 OWIN 宿主共享的有序路由表。</summary>
  public sealed class UiRouteTable
  {
    private readonly Dictionary<UiHttpMethod, List<(Regex regex, UiHandler handler)>> _routesByMethod
        = new Dictionary<UiHttpMethod, List<(Regex, UiHandler)>>();

    /// <summary>注册路由处理程序。</summary>
    public void Add(UiHttpMethod method, string pattern, UiHandler handler)
    {
      if (pattern == null) throw new ArgumentNullException(nameof(pattern));
      if (handler == null) throw new ArgumentNullException(nameof(handler));

      if (!_routesByMethod.TryGetValue(method, out var routes))
      {
        routes = new List<(Regex, UiHandler)>();
        _routesByMethod[method] = routes;
      }

      var regexPattern = UiRoutePattern.ToRegexPattern(pattern);
      routes.Add((
          new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled),
          handler));
    }

    /// <summary>为给定方法与路径匹配第一个已注册的路由。</summary>
    public bool TryMatch(string method, string path, out Match match, out UiHandler handler)
    {
      match = null;
      handler = null;
      if (string.IsNullOrEmpty(path))
        path = "/";

      if (!TryParseMethod(method, out var httpMethod))
        return false;

      if (TryMatchInternal(httpMethod, path, out match, out handler))
        return true;

      if (httpMethod == UiHttpMethod.Head)
        return TryMatchInternal(UiHttpMethod.Get, path, out match, out handler);

      return false;
    }

    private bool TryMatchInternal(UiHttpMethod httpMethod, string path, out Match match, out UiHandler handler)
    {
      match = null!;
      handler = null!;

      if (!_routesByMethod.TryGetValue(httpMethod, out var routes))
        return false;

      foreach (var route in routes)
      {
        var m = route.regex.Match(path);
        if (!m.Success)
          continue;

        match = m;
        handler = route.handler;
        return true;
      }

      return false;
    }

    private static bool TryParseMethod(string method, out UiHttpMethod httpMethod)
    {
      httpMethod = default;
      if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
      {
        httpMethod = UiHttpMethod.Get;
        return true;
      }

      if (string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
      {
        httpMethod = UiHttpMethod.Head;
        return true;
      }

      if (string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
      {
        httpMethod = UiHttpMethod.Post;
        return true;
      }

      if (string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase))
      {
        httpMethod = UiHttpMethod.Put;
        return true;
      }

      if (string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase))
      {
        httpMethod = UiHttpMethod.Delete;
        return true;
      }

      if (string.Equals(method, "PATCH", StringComparison.OrdinalIgnoreCase))
      {
        httpMethod = UiHttpMethod.Patch;
        return true;
      }

      if (string.Equals(method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
      {
        httpMethod = UiHttpMethod.Options;
        return true;
      }

      return false;
    }
  }
}
