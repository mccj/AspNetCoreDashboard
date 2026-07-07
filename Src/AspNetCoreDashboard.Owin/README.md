# AspNetCoreDashboard.Owin

OWIN **宿主包**，在 .NET Framework 4.8 应用中挂载 `IUiModule`：单中间件内完成授权、API 路由、嵌入静态资源与 SPA 回退。

| 属性 | 值 |
|------|-----|
| NuGet | `AspNetCoreDashboard.Owin` |
| TFM | `net48` |
| 依赖 | `AspNetCoreDashboard.Abstractions`、`Microsoft.Owin` |

## 功能概览

- **注册表**：`IAppBuilder.AddUiModules()` 返回共享 `UiModuleRegistry`
- **挂载**：`UseUiModules()` / `UseUiModule<T>()`
- **授权**：`IUiAuthorizationFilter`；可选 `IOwinAuthorizationAdapter` 桥接策略授权
- **路由**：`OwinUiRouteTable`，与 Abstractions 路由模板语义一致
- **静态资源**：中间件内读嵌入流，支持 ETag/304、Range
- **上传**：multipart `GetFormFileAsync`

## 使用方法

### 最小示例（Startup）

```csharp
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Owin.Extensions;

public void Configuration(IAppBuilder app)
{
    app.AddUiModules()
       .AddModule<MyUiModule>()
       .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());

    app.UseUiModules();
}
```

### 基于角色的授权

```csharp
// 在模块 Configure 中
builder.WithRequiredRole("Admin")
       .MapGet("/api/status", ctx => ctx.WriteAsync("ok"));
```

### 策略授权（需适配器）

OWIN 不直接使用 ASP.NET Core `IAuthorizationService`，需实现 `IOwinAuthorizationAdapter`：

```csharp
app.UseOwinAuthorizationAdapter(new MyPolicyAdapter());

app.AddUiModules()
   .SetAuthorizationPolicy("/MyModule", "AdminOnly");

app.UseUiModules();
```

## 与 ASP.NET Core 宿主的差异

| 能力 | ASP.NET Core | OWIN |
|------|--------------|------|
| 静态文件 | `EmbeddedFileProvider` + `UseStaticFiles` | 中间件内读嵌入流 |
| Policy 授权 | 原生 `SetAuthorizationPolicy` | 需 `IOwinAuthorizationAdapter` |
| 中间件结构 | 授权 → 静态 → 路由（三段） | 单中间件顺序处理 |

模块作者代码无需分支；差异由宿主包封装。

## 主要扩展方法

| 扩展 | 说明 |
|------|------|
| `AddUiModules` | 在 `IAppBuilder.Properties` 中创建注册表 |
| `UseUiModules` | 挂载注册表内全部模块 |
| `UseUiModule` / `UseUiModule<T>` | 挂载单个模块 |
| `UseOwinAuthorizationAdapter` | 注册策略授权适配器 |
| `SetAuthorizationPolicy`（扩展） | 结合适配器设置策略名 |
| `WithRequiredRole` | 模块内声明角色要求 |

## 注意事项

- 必须先 `AddUiModules()`，再 `UseUiModules()`；否则抛出“未找到注册表”异常。
- **不支持** ASP.NET Core 的 `SetAuthorizationPolicy` 直接调用（无适配器时）；请用 `SetAuthorization` + `RequireRoleAuthorizationFilter`，或实现 `IOwinAuthorizationAdapter`。
- `WithAuthorization(IUiAuthorizationFilter)` 在 Abstractions 层会检测宿主类型；模块内授权筛选器写法与 ASP.NET Core 一致。
- 自宿主示例见 [OwinSamples](../../Samples/AspNetCoreDashboardOwinSamples)（默认端口 1101）。

## 相关文档

- [架构说明](../../docs/ARCHITECTURE.md)（net8 与 net48 对比）
- [安全指南](../../docs/SECURITY.md)
