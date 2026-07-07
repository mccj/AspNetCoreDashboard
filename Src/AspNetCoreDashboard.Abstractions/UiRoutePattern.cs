using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// 编译路由模式。支持字面路径、旧版正则模式（例如 <c>/.*</c>）、
  /// 原始正则（以 <c>^</c> 为前缀）、模板参数（例如 <c>/api/items/{id}</c>）、
  /// 约束（<c>{id:int}</c>、<c>{id:guid}</c>）以及捕获全部（<c>{*path}</c>）。
  /// </summary>
  public static class UiRoutePattern
  {
    private static readonly Regex ParameterRegex = new Regex(
        @"\{(\*?)([a-zA-Z_][a-zA-Z0-9_]*)(?::([a-zA-Z]+))?\}",
        RegexOptions.Compiled);

    /// <summary>将路由模式编译为正则表达式字符串。</summary>
    public static string ToRegexPattern(string pattern)
    {
      if (pattern == null) throw new ArgumentNullException(nameof(pattern));
      if (pattern.StartsWith("^", StringComparison.Ordinal))
        return pattern;

      if (pattern.IndexOf('{') >= 0)
        return CompileTemplate(pattern);

      var regexPattern = pattern;
      if (!regexPattern.StartsWith("^", StringComparison.Ordinal))
        regexPattern = "^" + regexPattern;
      if (!regexPattern.EndsWith("$", StringComparison.Ordinal))
        regexPattern += "$";
      return regexPattern;
    }

    private static string CompileTemplate(string pattern)
    {
      var normalized = pattern.StartsWith("/", StringComparison.Ordinal) ? pattern : "/" + pattern;
      var builder = new StringBuilder("^");
      var lastIndex = 0;

      foreach (Match match in ParameterRegex.Matches(normalized))
      {
        if (match.Index > lastIndex)
          builder.Append(Regex.Escape(normalized.Substring(lastIndex, match.Index - lastIndex)));

        var isCatchAll = match.Groups[1].Value == "*";
        var name = match.Groups[2].Value;
        var constraint = match.Groups[3].Value;

        if (isCatchAll)
        {
          builder.Append("(?<").Append(name).Append(">.*)");
        }
        else
        {
          builder.Append("(?<").Append(name).Append(">").Append(GetConstraintPattern(constraint)).Append(')');
        }

        lastIndex = match.Index + match.Length;
      }

      if (lastIndex < normalized.Length)
        builder.Append(Regex.Escape(normalized.Substring(lastIndex)));

      builder.Append('$');
      return builder.ToString();
    }

    private static string GetConstraintPattern(string constraint)
    {
      if (string.IsNullOrEmpty(constraint))
        return "[^/]+";

      switch (constraint.ToLowerInvariant())
      {
        case "int":
          return @"\d+";
        case "guid":
          return @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        default:
          return "[^/]+";
      }
    }
  }
}
