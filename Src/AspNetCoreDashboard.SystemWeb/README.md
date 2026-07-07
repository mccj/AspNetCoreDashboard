# AspNetCoreDashboard.SystemWeb

经典 **ASP.NET System.Web** 的 HttpModule 适配器，在 `net48` IIS/自宿主应用中提供 `IUiModule` 挂载能力。

| 属性 | 值 |
|------|-----|
| NuGet | `AspNetCoreDashboard.SystemWeb` |
| TFM | `net48` |
| 依赖 | `AspNetCoreDashboard.Abstractions` |
| 状态 | 可用；复杂场景建议优先 OWIN 或 ASP.NET Core |

## 功能概览

- **`UiModuleHttpModule`**：在 `HttpApplication` 管道中拦截模块路径请求
- **`UiModuleHttpModuleRegistry`**：在 `Application_Start` 中注册模块
- 支持：嵌入静态文件、API 路由（`UiRouteTable`）、SPA fallback、`IUiAuthorizationFilter`
- 401/403 与 ASP.NET Core 语义一致（未认证 / 已认证但拒绝）

## 使用方法

### 1. 引用包并注册 HttpModule

`web.config`：

```xml
<system.webServer>
  <modules>
    <add name="UiModuleHttpModule" type="AspNetCoreDashboard.SystemWeb.UiModuleHttpModule, AspNetCoreDashboard.SystemWeb" />
  </modules>
</system.webServer>
```

### 2. Application_Start 注册模块

```csharp
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.SystemWeb;

protected void Application_Start()
{
    UiModuleHttpModuleRegistry.RegisterAll(new IUiModule[]
    {
        new MyUiModule()
    });
}
```

### 3. 模块实现（与 OWIN/Core 相同）

```csharp
public sealed class MyUiModule : IUiModule
{
    public string PathPrefix => "/MyModule";

    public void Configure(IUiModuleRegistration builder)
    {
        builder.MapEmbeddedUi(typeof(MyUiModule).Assembly, "MyModule.Content")
               .MapFallbackToIndex("MyModule.Content.index.html")
               .MapGet("/api/ping", ctx => ctx.WriteAsync("pong"));
    }
}
```

## 请求处理顺序

```
HTTP /{PathPrefix}/...
  → 授权筛选器
  → API 路由匹配
  → 嵌入静态资源
  → SPA fallback（index.html）
```

## 与 OWIN / ASP.NET Core 的差异

| 能力 | System.Web | OWIN / ASP.NET Core |
|------|------------|---------------------|
| 注册方式 | `Application_Start` + HttpModule | 中间件 / `UseUiModules` |
| Policy 授权 | 不支持 | ASP.NET Core 支持 |
| Range / 流式静态文件 | 内存缓冲 | 完整流式与 Range |
| `WithContentSecurityPolicy` 等 | 注册 API 存在，部分为 no-op | 完整支持 |

## 注意事项

- 必须在应用启动时完成 `Register` / `RegisterAll`；HttpModule 本身无 DI 集成。
- **不支持** ASP.NET Core Authorization Policy；请使用 `IUiAuthorizationFilter`。
- 反向代理与 Cookie 安全策略需自行在 IIS / 应用层配置。
- 新项目建议优先 [AspNetCoreDashboard.Owin](../AspNetCoreDashboard.Owin/README.md) 或 [AspNetCoreDashboard](../AspNetCoreDashboard/README.md)。
- 集成测试见 `AspNetCoreDashboard.SystemWeb.Tests`。

## 相关文档

- [架构说明](../../docs/ARCHITECTURE.md)
- [故障排查](../../docs/TROUBLESHOOTING.md)（System.Web 章节）
