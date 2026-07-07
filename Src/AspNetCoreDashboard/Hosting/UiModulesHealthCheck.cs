using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspNetCoreDashboard.Hosting
{
  /// <summary>报告所有已注册 UI 模块的健康状态。</summary>
  public sealed class UiModulesHealthCheck : IHealthCheck
  {
    private readonly IEnumerable<IUiModule> _modules;

    /// <summary>基于已注册模块创建健康检查。</summary>
    public UiModulesHealthCheck(IEnumerable<IUiModule> modules)
    {
      _modules = modules ?? new IUiModule[0];
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
      var summaries = new List<UiModuleSummary>();
      foreach (var module in _modules)
        summaries.Add(UiModuleDiagnostics.GetSummary(module));

      if (summaries.Count == 0)
        return Task.FromResult(HealthCheckResult.Healthy("未注册任何 UI 模块。"));

      var data = new Dictionary<string, object>();
      for (var i = 0; i < summaries.Count; i++)
      {
        var summary = summaries[i];
        data["module:" + i] = JsonSerializer.Serialize(new
        {
          summary.PathPrefix,
          summary.ModuleType,
          summary.RouteCount,
          summary.HasEmbeddedUi,
          summary.FallbackIndexResource
        });
      }

      var message = "UI 模块：" + string.Join(
          ", ",
          summaries.ConvertAll(s => $"{s.PathPrefix} ({s.ModuleType}, routes={s.RouteCount})"));

      return Task.FromResult(HealthCheckResult.Healthy(message, data));
    }
  }
}
