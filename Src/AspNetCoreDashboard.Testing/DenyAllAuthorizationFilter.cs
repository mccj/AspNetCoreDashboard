using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Testing
{
  /// <summary>始终拒绝访问的授权筛选器。适用于集成测试。</summary>
  public sealed class DenyAllAuthorizationFilter : IUiAuthorizationFilter
  {
    /// <inheritdoc />
    public bool Authorize(IUiContext context) => false;
  }
}
