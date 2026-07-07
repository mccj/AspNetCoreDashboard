using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCoreDashboard.Extensions
{
  /// <summary>UI 模块健康检查端点扩展。</summary>
  public static class UiModuleHealthCheckApplicationBuilderExtensions
  {
    /// <summary>映射包含模块明细 JSON 的健康检查端点。</summary>
    public static IEndpointConventionBuilder MapUiModulesHealthChecks(
        this WebApplication app,
        string path = "/health/modules")
    {
      return app.MapHealthChecks(path, new HealthCheckOptions
      {
        ResponseWriter = WriteJsonResponseAsync
      });
    }

    private static Task WriteJsonResponseAsync(HttpContext context, HealthReport report)
    {
      context.Response.ContentType = "application/json; charset=utf-8";

      var payload = new
      {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        entries = report.Entries.ToDictionary(
              entry => entry.Key,
              entry => new
              {
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data?.ToDictionary(
                      pair => pair.Key,
                      pair => pair.Value)
              })
      };

      return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
  }
}
