using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboardLibrarySamples
{
  /// <summary>
  /// 第二个 UI 模块，演示多模块挂载与 JSON 状态端点。
  /// </summary>
  [UiModule("/Diagnostics")]
  public sealed class DiagnosticsUiModule : IUiModule
  {
    public string PathPrefix => "/Diagnostics";

    public void Configure(IUiModuleRegistration builder)
    {
      builder.MapGet("/health", HandleHealth)
             .MapGet("/api/status", HandleStatus);
    }

    private static Task HandleHealth(IUiContext context)
    {
      UiSecurityHeaders.ApplyBaseline(context);
      return context.WriteAsync("ok");
    }

    private static Task HandleStatus(IUiContext context)
    {
      return context.WriteJsonAsync(new
      {
        ok = true,
        module = "Diagnostics",
        description = "Companion module registered via AddModulesFromAssembly"
      });
    }
  }
}
