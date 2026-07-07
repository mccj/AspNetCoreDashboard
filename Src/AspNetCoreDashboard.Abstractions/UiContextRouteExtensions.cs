using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary><see cref="IUiContext"/> 的路由匹配辅助方法。</summary>
  public static class UiContextRouteExtensions
  {
    /// <summary>从当前路由匹配结果中获取具名路由值。</summary>
    public static string? GetRouteValue(this IUiContext context, string name)
    {
      if (context == null || string.IsNullOrEmpty(name))
        return null;

      var match = context.RouteMatch;
      if (match == null || !match.Success)
        return null;

      return match.Groups[name]?.Success == true ? match.Groups[name].Value : null;
    }

    /// <summary>从当前路由匹配结果中获取所有具名路由值。</summary>
    public static IReadOnlyDictionary<string, string> GetRouteValues(this IUiContext context)
    {
      var values = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
      if (context?.RouteMatch == null || !context.RouteMatch.Success)
        return values;

      foreach (var groupName in RegexMatchHelper.GetSuccessfulGroupNames(context.RouteMatch))
      {
        values[groupName] = context.RouteMatch.Groups[groupName].Value;
      }

      return values;
    }
  }

  internal static class RegexMatchHelper
  {
    public static IEnumerable<string> GetSuccessfulGroupNames(Match match)
    {
      var regex = GetRegex(match);
      if (regex != null)
      {
        foreach (var name in regex.GetGroupNames())
        {
          if (name == "0" || !match.Groups[name].Success)
            continue;

          yield return name;
        }

        yield break;
      }

      for (var i = 1; i < match.Groups.Count; i++)
      {
        if (match.Groups[i].Success)
          yield return i.ToString();
      }
    }

    private static Regex? GetRegex(Match match)
    {
      var regexProperty = typeof(Match).GetProperty("Regex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
      if (regexProperty?.GetValue(match) is Regex regexFromProperty)
        return regexFromProperty;

      var regexField = typeof(Match).GetField("_regex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
      return regexField?.GetValue(match) as Regex;
    }
  }
}
