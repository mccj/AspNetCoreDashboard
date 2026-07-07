using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreDashboard.Hosting
{
  internal sealed class UiModulePipelineMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly UiModuleHostingOptions _options;
    private readonly UiModuleDescriptor _descriptor;
    private readonly ILogger<UiModulePipelineMiddleware> _logger;

    public UiModulePipelineMiddleware(
        RequestDelegate next,
        IOptions<UiModuleHostingOptions> options,
        UiModuleDescriptor descriptor,
        ILogger<UiModulePipelineMiddleware> logger)
    {
      _next = next ?? throw new ArgumentNullException(nameof(next));
      _options = options?.Value ?? new UiModuleHostingOptions();
      _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
      _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      using (var activity = UiModuleActivitySource.Instance.StartActivity(
          "UiModule.Request",
          ActivityKind.Server))
      {
        activity?.SetTag("ui.module.path_prefix", _descriptor.PathPrefix);
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.route", context.Request.Path.Value);

        if (_options.ApplySecurityHeaders)
        {
          var uiContext = new AspNetCoreUiContext(context);
          var csp = !string.IsNullOrEmpty(_descriptor.ContentSecurityPolicy)
              ? _descriptor.ContentSecurityPolicy
              : _options.ContentSecurityPolicy;
          UiSecurityHeaders.ApplyBaseline(uiContext, csp);
        }

        var maxBytes = _descriptor.MaxUploadBytes > 0
            ? _descriptor.MaxUploadBytes
            : _options.MaxRequestBodyBytes;
        if (maxBytes > 0 &&
            context.Request.ContentLength.HasValue &&
            context.Request.ContentLength.Value > maxBytes)
        {
          context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
          activity?.SetTag("http.status_code", 413);
          return;
        }

        if (!_options.EnableRequestLogging)
        {
          await _next(context);
          activity?.SetTag("http.status_code", context.Response.StatusCode);
          return;
        }

        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();
        activity?.SetTag("http.status_code", context.Response.StatusCode);
        _logger?.LogInformation(
            "UI module {PathPrefix} {Method} {Path} -> {StatusCode} in {ElapsedMs}ms",
            _descriptor.PathPrefix,
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
      }
    }
  }
}
