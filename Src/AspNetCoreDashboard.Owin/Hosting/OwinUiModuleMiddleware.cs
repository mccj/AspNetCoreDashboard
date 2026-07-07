using System;
using System.IO;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.Owin;

namespace AspNetCoreDashboard.Owin.Hosting
{
  internal sealed class OwinUiModuleMiddleware : OwinMiddleware
  {
    private readonly OwinUiModuleDescriptor _descriptor;
    private readonly UiRouteTable _routeTable;

    public OwinUiModuleMiddleware(OwinMiddleware next, OwinUiModuleDescriptor descriptor) : base(next)
    {
      _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
      _routeTable = OwinUiRouteTableBuilder.Build(descriptor);
    }

    public override async Task Invoke(IOwinContext context)
    {
      await BufferRequestBodyIfNeededAsync(context);

      var path = context.Request.Path.Value ?? string.Empty;
      var uiContext = new OwinUiContext(context);

      foreach (var filter in _descriptor.AuthorizationFilters)
      {
        if (!filter.Authorize(uiContext))
        {
          var isAuthenticated = context.Request.User?.Identity?.IsAuthenticated == true;
          context.Response.StatusCode = isAuthenticated ? 403 : 401;
          return;
        }
      }

      if (_routeTable.TryMatch(context.Request.Method, path, out var match, out var handler))
      {
        uiContext.RouteMatch = match;
        var isHead = string.Equals(context.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase);
        if (isHead)
        {
          using (var buffer = new MemoryStream())
          {
            var originalBody = context.Response.Body;
            context.Response.Body = buffer;
            await handler(uiContext);
            context.Response.Body = originalBody;
            context.Response.Headers["Content-Length"] = buffer.Length.ToString();
          }

          return;
        }

        await handler(uiContext);
        return;
      }

      if (_descriptor.HasEmbeddedUi)
      {
        var relativePath = path.TrimStart('/');
        if (string.IsNullOrEmpty(relativePath))
          relativePath = "index.html";

        var resourceName = EmbeddedResourcePathHelper.ToResourceName(_descriptor.EmbeddedBaseNamespace, relativePath);
        using (var stream = EmbeddedResourceCache.OpenReadStream(_descriptor.EmbeddedAssembly, resourceName))
        {
          if (stream != null)
          {
            context.Response.ContentType = OwinEmbeddedResourceWriter.GuessContentType(relativePath);
            OwinEmbeddedResourceWriter.ApplyCacheHeaders(context.Response, relativePath, _descriptor.StaticCacheMaxAgeSeconds);
            var etag = stream is MemoryStream memory && memory.TryGetBuffer(out var segment) && segment.Array != null
                ? EmbeddedResourceEtag.Compute(segment.Array)
                : ComputeEtagAndReset(stream);
            context.Response.Headers["ETag"] = etag;
            if (!IsNotModified(context, etag))
            {
              if (stream.CanSeek)
                stream.Position = 0;
              await stream.CopyToAsync(context.Response.Body);
            }
            else
            {
              context.Response.StatusCode = 304;
            }

            return;
          }
        }
      }

      if (OwinSpaFallbackHelper.ShouldFallback(path, _descriptor.FallbackIndexResource, _descriptor.SpaFallbackExcludedExtensions))
      {
        context.Response.Headers["Cache-Control"] = "no-cache";
        await OwinEmbeddedResourceWriter.WriteResourceAsync(context.Request, context.Response, _descriptor.EmbeddedAssembly, _descriptor.FallbackIndexResource, "text/html");
        return;
      }

      await Next.Invoke(context);
    }

    private static string ComputeEtagAndReset(Stream stream)
    {
      if (stream.CanSeek)
      {
        var position = stream.Position;
        var etag = EmbeddedResourceEtag.Compute(stream);
        stream.Position = position;
        return etag;
      }

      return EmbeddedResourceEtag.Compute(stream);
    }

    private static bool IsNotModified(IOwinContext context, string etag)
    {
      var header = context.Request.Headers.Get("If-None-Match");
      return !string.IsNullOrEmpty(header) && string.Equals(header.Trim(), etag, StringComparison.Ordinal);
    }

    private static async Task BufferRequestBodyIfNeededAsync(IOwinContext context)
    {
      const string bufferKey = "AspNetCoreDashboard.RequestBodyBuffer";
      if (context.Environment.ContainsKey(bufferKey))
        return;

      if (!string.Equals(context.Request.Method, "POST", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(context.Request.Method, "PUT", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(context.Request.Method, "PATCH", StringComparison.OrdinalIgnoreCase))
      {
        return;
      }

      using (var memory = new MemoryStream())
      {
        await context.Request.Body.CopyToAsync(memory);
        var bytes = memory.ToArray();
        context.Environment[bufferKey] = bytes;
        context.Request.Body = new MemoryStream(bytes, writable: false);
      }
    }
  }
}
