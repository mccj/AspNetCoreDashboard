namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>跟踪已挂载的模块路径前缀，防止重复挂载。</summary>
  public interface IUiModuleMountTracker
  {
    /// <summary>注册路径前缀；若已挂载则抛出异常。</summary>
    void Register(string pathPrefix);

    /// <summary>清除已跟踪的路径前缀。供测试宿主使用。</summary>
    void Reset();
  }
}
