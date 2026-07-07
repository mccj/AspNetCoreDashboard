namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// 定义可由宿主应用程序挂载的可嵌入 UI 模块。
  /// </summary>
  public interface IUiModule
  {
    /// <summary>
    /// 本模块的 URL 路径前缀（例如 <c>"/Dashboard"</c>）。
    /// </summary>
    string PathPrefix { get; }

    /// <summary>
    /// 注册本模块的静态资源与 API 处理程序。
    /// </summary>
    void Configure(IUiModuleRegistration builder);
  }
}
