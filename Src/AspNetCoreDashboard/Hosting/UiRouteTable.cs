using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace AspNetCoreDashboard.Hosting
{
  internal static class UiRouteTableBuilder
  {
    public static UiRouteTable Build(UiModuleDescriptor descriptor)
    {
      var table = new UiRouteTable();
      foreach (var route in descriptor.Routes)
        table.Add(route.Method, route.Pattern, route.Handler);
      return table;
    }
  }

  internal static class EmbeddedResourceWriter
  {
    private static readonly FileExtensionContentTypeProvider ContentTypes = new FileExtensionContentTypeProvider();

    public static async Task WriteAsync(HttpResponse response, Assembly assembly, string resourceName, string contentType)
    {
      await WriteAsync(response.HttpContext, assembly, resourceName, contentType);
    }

    public static async Task WriteAsync(HttpContext context, Assembly assembly, string resourceName, string contentType)
    {
      using (var stream = EmbeddedResourceCache.OpenReadStream(assembly, resourceName))
      {
        if (stream == null)
        {
          context.Response.StatusCode = StatusCodes.Status404NotFound;
          return;
        }

        var etag = ComputeEtag(stream);
        context.Response.Headers.ETag = etag;
        if (IsNotModified(context.Response, etag))
        {
          context.Response.StatusCode = StatusCodes.Status304NotModified;
          return;
        }

        context.Response.ContentType = contentType ?? "application/octet-stream";
        context.Response.Headers.AcceptRanges = "bytes";

        if (stream.CanSeek && await TryWriteRangeAsync(context, stream))
          return;

        await stream.CopyToAsync(context.Response.Body, context.RequestAborted);
      }
    }

    public static string GuessContentType(string path)
    {
      if (string.IsNullOrEmpty(path))
        return "text/html";

      return ContentTypes.TryGetContentType(path, out var contentType)
          ? contentType
          : "application/octet-stream";
    }

    private static string ComputeEtag(Stream stream)
    {
      if (stream is MemoryStream memory && memory.TryGetBuffer(out var segment) && segment.Array != null)
        return EmbeddedResourceEtag.Compute(segment.Array);

      if (stream.CanSeek)
      {
        var position = stream.Position;
        var etag = EmbeddedResourceEtag.Compute(stream);
        stream.Position = position;
        return etag;
      }

      using (var copy = new MemoryStream())
      {
        stream.CopyTo(copy);
        return EmbeddedResourceEtag.Compute(copy.ToArray());
      }
    }

    private static async Task<bool> TryWriteRangeAsync(HttpContext context, Stream stream)
    {
      if (!context.Request.Headers.TryGetValue("Range", out var values))
        return false;

      var rangeHeader = values.ToString();
      if (!rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        return false;

      var rangeValue = rangeHeader.Substring("bytes=".Length).Trim();
      var dash = rangeValue.IndexOf('-');
      if (dash < 0)
        return false;

      if (!long.TryParse(rangeValue.Substring(0, dash), out var start))
        start = 0;

      var endPart = rangeValue.Substring(dash + 1);
      var total = stream.Length;
      long end = string.IsNullOrEmpty(endPart) ? total - 1 : long.Parse(endPart);
      if (start < 0 || end >= total || start > end)
      {
        context.Response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
        context.Response.Headers.ContentRange = "bytes */" + total;
        return true;
      }

      var length = end - start + 1;
      stream.Position = start;
      context.Response.StatusCode = StatusCodes.Status206PartialContent;
      context.Response.Headers.ContentRange = $"bytes {start}-{end}/{total}";
      context.Response.ContentLength = length;
      var buffer = new byte[81920];
      var remaining = length;
      while (remaining > 0)
      {
        var read = await stream.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, remaining), context.RequestAborted);
        if (read <= 0)
          break;

        await context.Response.Body.WriteAsync(buffer, 0, read, context.RequestAborted);
        remaining -= read;
      }

      return true;
    }

    private static bool IsNotModified(HttpResponse response, string etag)
    {
      if (!response.HttpContext.Request.Headers.TryGetValue("If-None-Match", out var values))
        return false;

      foreach (var value in values)
      {
        if (string.Equals(value, etag, StringComparison.Ordinal))
          return true;
      }

      return false;
    }
  }

  internal static class SpaFallbackHelper
  {
    public static bool ShouldFallback(string path, string fallbackResource, System.Collections.Generic.IList<string> excludedExtensions)
    {
      if (string.IsNullOrEmpty(fallbackResource))
        return false;

      if (path == "/" || string.IsNullOrEmpty(path))
        return true;

      if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        return false;

      var extension = Path.GetExtension(path);
      if (!string.IsNullOrEmpty(extension) && excludedExtensions != null)
      {
        foreach (var excluded in excludedExtensions)
        {
          if (string.Equals(extension, excluded, StringComparison.OrdinalIgnoreCase))
            return false;
        }
      }

      return true;
    }
  }
}
