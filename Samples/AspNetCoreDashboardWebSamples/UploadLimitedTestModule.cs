using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboardWebSamples;

/// <summary>集成测试用：限制请求体大小的 UI 模块。</summary>
public sealed class UploadLimitedTestModule : IUiModule
{
  public string PathPrefix => "/UploadLimited";

  public void Configure(IUiModuleRegistration builder)
  {
    builder.WithMaxUploadBytes(32)
           .MapPost("/api/echo", HandleEcho);
  }

  private static async Task HandleEcho(IUiContext context)
  {
    var body = await context.ReadBodyAsStringAsync();
    await context.WriteAsync(body ?? string.Empty, "text/plain");
  }
}
