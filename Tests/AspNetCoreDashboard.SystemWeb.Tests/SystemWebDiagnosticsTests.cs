using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.SystemWeb;
using AspNetCoreDashboardLibrarySamples;
using Xunit;

namespace AspNetCoreDashboard.SystemWeb.Tests
{
  public sealed class SystemWebDiagnosticsTests
  {
    [Fact]
    public void GetSummary_reports_sample_module_routes()
    {
      var summary = UiModuleDiagnostics.GetSummary(new SampleUiModule());
      Assert.Equal("/Dashboard", summary.PathPrefix);
      Assert.True(summary.HasEmbeddedUi);
      Assert.True(summary.RouteCount > 0);
    }
  }
}
