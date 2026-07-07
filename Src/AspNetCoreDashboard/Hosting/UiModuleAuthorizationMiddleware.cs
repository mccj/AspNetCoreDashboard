using System;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreDashboard.Hosting
{
  internal sealed class UiModuleAuthorizationMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IUiAuthorizationFilter[] _filters;

    public UiModuleAuthorizationMiddleware(RequestDelegate next, UiModuleAuthorizationOptions options)
    {
      _next = next ?? throw new ArgumentNullException(nameof(next));
      _filters = options?.Filters ?? Array.Empty<IUiAuthorizationFilter>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
      if (_filters.Length == 0)
      {
        await _next(context);
        return;
      }

      var uiContext = new AspNetCoreUiContext(context);
      foreach (var filter in _filters)
      {
        if (!filter.Authorize(uiContext))
        {
          var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
          context.Response.StatusCode = isAuthenticated
              ? StatusCodes.Status403Forbidden
              : StatusCodes.Status401Unauthorized;
          return;
        }
      }

      await _next(context);
    }
  }
}
