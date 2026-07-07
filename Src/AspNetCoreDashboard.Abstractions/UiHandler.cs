using System.Threading.Tasks;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// 处理 UI 模块 HTTP 请求的委托。
  /// </summary>
  public delegate Task UiHandler(IUiContext context);
}
