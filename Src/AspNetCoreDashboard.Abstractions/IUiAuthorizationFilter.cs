namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// UI 模块请求的授权筛选器。
  /// </summary>
  public interface IUiAuthorizationFilter
  {
    /// <summary>判断请求是否已授权。</summary>
    bool Authorize(IUiContext context);
  }
}
