using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboardWebSamples
{
  internal sealed class DenyAllAuthorizationFilter : IUiAuthorizationFilter
  {
    public bool Authorize(IUiContext context) => false;
  }
}
