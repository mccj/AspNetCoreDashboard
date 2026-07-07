using System.Reflection;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;

namespace MyUiModule;

/// <summary>
/// 示例 UI 模块：嵌入静态页面并提供简单 API。
/// </summary>
[UiModule("/MyModule")]
public sealed class MyUiModule : IUiModule
{
    public string PathPrefix => "/MyModule";

    public void Configure(IUiModuleRegistration builder)
    {
        var assembly = typeof(MyUiModule).Assembly;
        builder.MapEmbeddedUi(assembly, "MyUiModule.Content")
               .MapFallbackToIndex("MyUiModule.Content.index.html")
               .MapGet("/api/ping", ctx => ctx.WriteAsync("pong"));
    }
}
