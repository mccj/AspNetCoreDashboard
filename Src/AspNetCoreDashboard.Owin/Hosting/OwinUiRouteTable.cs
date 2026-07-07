using System;
using System.IO;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Owin.Hosting
{
  internal static class OwinUiRouteTableBuilder
  {
    public static UiRouteTable Build(OwinUiModuleDescriptor descriptor)
    {
      var table = new UiRouteTable();
      foreach (var route in descriptor.Routes)
        table.Add(route.Method, route.Pattern, route.Handler);
      return table;
    }
  }

  internal static class OwinEmbeddedResourceWriter
  {
    public static async Task WriteEmbeddedPathAsync(Microsoft.Owin.IOwinResponse response, System.Reflection.Assembly assembly, string baseNamespace, string relativePath)
    {
      var resourceName = EmbeddedResourcePathHelper.ToResourceName(baseNamespace, relativePath);
      using (var stream = EmbeddedResourceCache.OpenReadStream(assembly, resourceName))
      {
        if (stream == null)
        {
          response.StatusCode = 404;
          return;
        }

        response.ContentType = OwinContentTypeResolver.GetContentType(relativePath);
        response.Headers["Accept-Ranges"] = "bytes";
        await stream.CopyToAsync(response.Body);
      }
    }

    public static async Task WriteResourceAsync(Microsoft.Owin.IOwinRequest request, Microsoft.Owin.IOwinResponse response, System.Reflection.Assembly assembly, string resourceName, string contentType)
    {
      using (var stream = EmbeddedResourceCache.OpenReadStream(assembly, resourceName))
      {
        if (stream == null)
        {
          response.StatusCode = 404;
          return;
        }

        var etag = ComputeEtag(stream);
        response.Headers["ETag"] = etag;
        if (IsNotModified(request, etag))
        {
          response.StatusCode = 304;
          return;
        }

        response.ContentType = contentType;
        response.Headers["Accept-Ranges"] = "bytes";
        await stream.CopyToAsync(response.Body);
      }
    }

    internal static string GuessContentType(string path) => OwinContentTypeResolver.GetContentType(path);

    internal static void ApplyCacheHeaders(Microsoft.Owin.IOwinResponse response, string relativePath, int maxAgeSeconds)
    {
      var fileName = System.IO.Path.GetFileName(relativePath);
      if (string.IsNullOrEmpty(fileName) ||
          fileName.Equals("index.html", StringComparison.OrdinalIgnoreCase))
      {
        response.Headers["Cache-Control"] = "no-cache";
        return;
      }

      response.Headers["Cache-Control"] = "public,max-age=" + maxAgeSeconds;
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

    private static bool IsNotModified(Microsoft.Owin.IOwinRequest request, string etag)
    {
      var header = request.Headers.Get("If-None-Match");
      return !string.IsNullOrEmpty(header) && string.Equals(header.Trim(), etag, StringComparison.Ordinal);
    }
  }

  internal static class OwinSpaFallbackHelper
  {
    public static bool ShouldFallback(string path, string fallbackResource, System.Collections.Generic.IList<string> excludedExtensions)
    {
      if (string.IsNullOrEmpty(fallbackResource))
        return false;

      if (path == "/" || string.IsNullOrEmpty(path))
        return true;

      if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        return false;

      var extension = System.IO.Path.GetExtension(path);
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
