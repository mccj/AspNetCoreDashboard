using System;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreDashboard.Hosting
{
  internal sealed class EmbeddedStaticFileMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly UiModuleDescriptor _descriptor;

    public EmbeddedStaticFileMiddleware(RequestDelegate next, UiModuleDescriptor descriptor)
    {
      _next = next ?? throw new ArgumentNullException(nameof(next));
      _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
    }

    public async Task InvokeAsync(HttpContext context)
    {
      if (!_descriptor.HasEmbeddedUi)
      {
        await _next(context);
        return;
      }

      var method = context.Request.Method;
      if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
      {
        await _next(context);
        return;
      }

      var path = context.Request.Path.Value ?? string.Empty;
      var relativePath = path.TrimStart('/');
      if (string.IsNullOrEmpty(relativePath))
        relativePath = "index.html";

      var resourceName = EmbeddedResourcePathHelper.ToResourceName(
          _descriptor.EmbeddedBaseNamespace,
          relativePath);

      using (var stream = EmbeddedResourceCache.OpenReadStream(_descriptor.EmbeddedAssembly, resourceName))
      {
        if (stream == null)
        {
          await _next(context);
          return;
        }

        var contentType = EmbeddedResourceWriter.GuessContentType(relativePath);
        ApplyCacheHeaders(context, relativePath);

        if (string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
        {
          var originalBody = context.Response.Body;
          using (var buffer = new System.IO.MemoryStream())
          {
            context.Response.Body = buffer;
            await EmbeddedResourceWriter.WriteAsync(
                context,
                _descriptor.EmbeddedAssembly,
                resourceName,
                contentType);
            context.Response.Body = originalBody;
            context.Response.ContentLength = buffer.Length;
          }

          return;
        }

        await EmbeddedResourceWriter.WriteAsync(
            context,
            _descriptor.EmbeddedAssembly,
            resourceName,
            contentType);
      }
    }

    private void ApplyCacheHeaders(HttpContext context, string relativePath)
    {
      var fileName = System.IO.Path.GetFileName(relativePath);
      if (string.IsNullOrEmpty(fileName) ||
          fileName.Equals("index.html", StringComparison.OrdinalIgnoreCase))
      {
        context.Response.Headers.CacheControl = "no-cache";
        return;
      }

      context.Response.Headers.CacheControl =
          "public,max-age=" + _descriptor.StaticCacheMaxAgeSeconds;
    }
  }
}
