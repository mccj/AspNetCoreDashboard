using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Hosting
{
  /// <summary>
  /// 包装类，避免中间件激活时将筛选器视为 DI 服务。
  /// </summary>
  internal sealed class UiModuleAuthorizationOptions
  {
    public UiModuleAuthorizationOptions(IUiAuthorizationFilter[] filters)
    {
      Filters = filters ?? System.Array.Empty<IUiAuthorizationFilter>();
    }

    public IUiAuthorizationFilter[] Filters { get; }
  }
}
