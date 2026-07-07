using System;
using System.IO;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreDashboard.Hosting
{
  internal sealed class AspNetCoreUiModuleMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly UiModuleDescriptor _descriptor;
    private readonly UiRouteTable _routeTable;

    public AspNetCoreUiModuleMiddleware(RequestDelegate next, UiModuleDescriptor descriptor)
    {
      _next = next ?? throw new ArgumentNullException(nameof(next));
      _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
      _routeTable = UiRouteTableBuilder.Build(descriptor);
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var path = context.Request.Path.Value ?? string.Empty;

      if (_routeTable.TryMatch(context.Request.Method, path, out var match, out var handler))
      {
        var isHead = string.Equals(context.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase);
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        var uiContext = new AspNetCoreUiContext(context) { RouteMatch = match };
        await handler(uiContext);

        context.Response.Body = originalBody;
        context.Response.Headers.ContentLength = buffer.Length;
        if (!isHead)
        {
          buffer.Position = 0;
          await buffer.CopyToAsync(originalBody, context.RequestAborted);
        }

        return;
      }

      if (SpaFallbackHelper.ShouldFallback(path, _descriptor.FallbackIndexResource, _descriptor.SpaFallbackExcludedExtensions))
      {
        context.Response.Headers.CacheControl = "no-cache";
        await EmbeddedResourceWriter.WriteAsync(
            context,
            _descriptor.EmbeddedAssembly,
            _descriptor.FallbackIndexResource,
            "text/html");
        return;
      }

      await _next(context);
    }
  }

  internal static class AspNetCoreUiModuleBranchBuilder
  {
    public static void ConfigureBranch(IApplicationBuilder branch, UiModuleDescriptor descriptor, IUiAuthorizationFilter[] filters)
    {
      branch.UseMiddleware<UiModulePipelineMiddleware>(descriptor);
      branch.UseMiddleware<UiModuleAuthorizationMiddleware>(new UiModuleAuthorizationOptions(filters));

      if (descriptor.HasEmbeddedUi)
      {
        branch.UseMiddleware<EmbeddedStaticFileMiddleware>(descriptor);
      }

      branch.UseMiddleware<AspNetCoreUiModuleMiddleware>(descriptor);
    }
  }
}
