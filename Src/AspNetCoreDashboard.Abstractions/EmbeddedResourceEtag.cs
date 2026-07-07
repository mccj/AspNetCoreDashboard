using System;
using System.IO;
using System.Security.Cryptography;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>嵌入式资源的 ETag 辅助方法。</summary>
  public static class EmbeddedResourceEtag
  {
    /// <summary>根据资源字节计算弱 ETag。</summary>
    public static string Compute(byte[] bytes)
    {
      if (bytes == null || bytes.Length == 0)
        return "\"0\"";

      using (var sha = SHA256.Create())
      {
        var hash = sha.ComputeHash(bytes);
        return FormatHash(hash);
      }
    }

    /// <summary>通过读取可定位流计算弱 ETag。</summary>
    public static string Compute(Stream stream)
    {
      if (stream == null) throw new ArgumentNullException(nameof(stream));

      using (var sha = SHA256.Create())
      {
        var hash = stream.CanSeek
            ? sha.ComputeHash(stream)
            : sha.ComputeHash(ReadAllBytes(stream));
        return FormatHash(hash);
      }
    }

    private static byte[] ReadAllBytes(Stream stream)
    {
      using (var memory = new MemoryStream())
      {
        stream.CopyTo(memory);
        return memory.ToArray();
      }
    }

    private static string FormatHash(byte[] hash)
    {
      return "\""
          + BitConverter.ToString(hash, 0, 8).Replace("-", string.Empty)
          + "\"";
    }

    /// <summary>当请求的 If-None-Match 头与 ETag 匹配时返回 true。</summary>
    public static bool IsNotModified(IUiContext context, string etag)
    {
      if (context == null || string.IsNullOrEmpty(etag))
        return false;

      var header = context.GetRequestHeader("If-None-Match");
      if (string.IsNullOrEmpty(header))
        return false;

      return string.Equals(header.Trim(), etag, StringComparison.Ordinal);
    }
  }
}
