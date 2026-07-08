using System;
using System.Collections.Generic;
using System.Text;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Owin.Hosting
{
  internal static class OwinMultipartFormReader
  {
    public static IUiFormFile TryGetFile(string contentType, byte[] body, string fieldName)
    {
      if (body == null || body.Length == 0 || string.IsNullOrEmpty(fieldName))
        return null;

      var boundary = GetBoundary(contentType);
      if (string.IsNullOrEmpty(boundary))
        return null;

      var delimiter = Encoding.UTF8.GetBytes("--" + boundary);
      var segments = Split(body, delimiter);
      foreach (var segment in segments)
      {
        if (segment.Length == 0)
          continue;

        var offset = 0;
        while (offset + 1 < segment.Length && segment[offset] == 13 && segment[offset + 1] == 10)
          offset += 2;

        if (offset >= segment.Length)
          continue;

        var headerEnd = IndexOf(segment, offset, new byte[] { 13, 10, 13, 10 });
        if (headerEnd < 0)
          continue;

        var headers = Encoding.UTF8.GetString(segment, offset, headerEnd - offset);
        if (!TryGetContentDisposition(headers, out var name, out var fileName))
          continue;

        if (!string.Equals(name, fieldName, StringComparison.OrdinalIgnoreCase))
          continue;

        var contentStart = headerEnd + 4;
        var contentLength = segment.Length - contentStart;
        while (contentLength >= 2 &&
               segment[contentStart + contentLength - 2] == 13 &&
               segment[contentStart + contentLength - 1] == 10)
        {
          contentLength -= 2;
        }

        while (contentLength >= 2 &&
               segment[contentStart + contentLength - 2] == 45 &&
               segment[contentStart + contentLength - 1] == 45)
        {
          contentLength -= 2;
        }

        if (contentLength <= 0)
          return null;

        var fileBytes = new byte[contentLength];
        Buffer.BlockCopy(segment, contentStart, fileBytes, 0, contentLength);
        var partContentType = GetHeaderValue(headers, "Content-Type");
        return new OwinFormFile(fileName ?? name ?? fieldName, partContentType, fileBytes);
      }

      return null;
    }

    private static string GetBoundary(string contentType)
    {
      const string marker = "boundary=";
      var index = contentType.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
      if (index < 0)
        return null;

      var boundary = contentType.Substring(index + marker.Length).Trim();
      var semi = boundary.IndexOf(';');
      if (semi >= 0)
        boundary = boundary.Substring(0, semi).Trim();

      if (boundary.StartsWith("\"", StringComparison.Ordinal) && boundary.EndsWith("\"", StringComparison.Ordinal))
        boundary = boundary.Substring(1, boundary.Length - 2);

      return boundary;
    }

    private static bool TryGetContentDisposition(string headers, out string name, out string fileName)
    {
      name = null;
      fileName = null;
      foreach (var line in headers.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
      {
        if (!line.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase))
          continue;

        var value = line.Substring("Content-Disposition:".Length).Trim();
        name = GetDispositionValue(value, "name");
        fileName = GetDispositionValue(value, "filename");
        return !string.IsNullOrEmpty(name);
      }

      return false;
    }

    private static string GetDispositionValue(string value, string key)
    {
      var marker = key + "=\"";
      var index = value.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
      if (index < 0)
        return null;

      var start = index + marker.Length;
      var end = value.IndexOf('"', start);
      return end > start ? value.Substring(start, end - start) : null;
    }

    private static string GetHeaderValue(string headers, string headerName)
    {
      foreach (var line in headers.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
      {
        if (!line.StartsWith(headerName + ":", StringComparison.OrdinalIgnoreCase))
          continue;

        return line.Substring(headerName.Length + 1).Trim();
      }

      return null;
    }

    private static List<byte[]> Split(byte[] source, byte[] delimiter)
    {
      var segments = new List<byte[]>();
      var start = 0;
      for (var i = 0; i <= source.Length - delimiter.Length; i++)
      {
        if (!StartsWith(source, i, delimiter))
          continue;

        if (i > start)
          segments.Add(CopyRange(source, start, i - start));

        start = i + delimiter.Length;
        i = start - 1;
      }

      if (start < source.Length)
        segments.Add(CopyRange(source, start, source.Length - start));

      return segments;
    }

    private static bool StartsWith(byte[] source, int offset, byte[] value)
    {
      for (var i = 0; i < value.Length; i++)
      {
        if (source[offset + i] != value[i])
          return false;
      }

      return true;
    }

    private static int IndexOf(byte[] source, int start, byte[] pattern)
    {
      for (var i = start; i <= source.Length - pattern.Length; i++)
      {
        if (StartsWith(source, i, pattern))
          return i;
      }

      return -1;
    }

    private static int IndexOf(byte[] source, byte[] pattern) => IndexOf(source, 0, pattern);

    private static byte[] CopyRange(byte[] source, int start, int length)
    {
      var copy = new byte[length];
      Buffer.BlockCopy(source, start, copy, 0, length);
      return copy;
    }
  }
}
