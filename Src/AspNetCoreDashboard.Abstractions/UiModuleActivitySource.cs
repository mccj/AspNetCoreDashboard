using System.Diagnostics;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>UI 模块请求的 OpenTelemetry 活动源。</summary>
  public static class UiModuleActivitySource
  {
    /// <summary>UI 模块托管的活动源。</summary>
    public static ActivitySource Instance { get; } = new ActivitySource("AspNetCoreDashboard.UiModules");
  }
}
