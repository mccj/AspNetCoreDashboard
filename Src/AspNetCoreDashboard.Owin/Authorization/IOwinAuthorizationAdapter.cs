using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Owin.Authorization
{
  /// <summary>将 ASP.NET Core 风格的授权策略名称映射为 OWIN 授权决策。</summary>
  public interface IOwinAuthorizationAdapter
  {
    /// <summary>针对当前 UI 模块请求评估指定策略。</summary>
    bool Authorize(string policyName, IUiContext context);
  }
}
