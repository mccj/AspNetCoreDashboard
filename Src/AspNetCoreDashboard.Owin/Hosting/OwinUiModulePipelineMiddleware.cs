using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.Owin;

namespace AspNetCoreDashboard.Owin.Hosting
{
  internal sealed class OwinUiModulePipelineMiddleware : OwinMiddleware
  {
    private readonly OwinUiModuleDescriptor _descriptor;
    private readonly long _defaultMaxUploadBytes;

    public OwinUiModulePipelineMiddleware(
        OwinMiddleware next,
        OwinUiModuleDescriptor descriptor) : base(next)
    {
      _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
      _defaultMaxUploadBytes = 0;
    }

    internal OwinUiModulePipelineMiddleware(
        OwinMiddleware next,
        OwinUiModuleDescriptor descriptor,
        long defaultMaxUploadBytes) : base(next)
    {
      _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
      _defaultMaxUploadBytes = defaultMaxUploadBytes;
    }

    public override async Task Invoke(IOwinContext context)
    {
      using (var activity = UiModuleActivitySource.Instance.StartActivity(
          "UiModule.Request",
          ActivityKind.Server))
      {
        activity?.SetTag("ui.module.path_prefix", _descriptor.PathPrefix);
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.route", context.Request.Path.Value);

        if (!string.IsNullOrEmpty(_descriptor.ContentSecurityPolicy))
        {
          var uiContext = new OwinUiContext(context);
          UiSecurityHeaders.ApplyBaseline(uiContext, _descriptor.ContentSecurityPolicy);
        }

        var maxBytes = _descriptor.MaxUploadBytes > 0
            ? _descriptor.MaxUploadBytes
            : _defaultMaxUploadBytes;

        if (maxBytes > 0 &&
            long.TryParse(context.Request.Headers["Content-Length"], out var contentLength) &&
            contentLength > maxBytes)
        {
          context.Response.StatusCode = 413;
          activity?.SetTag("http.status_code", 413);
          return;
        }

        await Next.Invoke(context);
        activity?.SetTag("http.status_code", context.Response.StatusCode);
      }
    }
  }
}
