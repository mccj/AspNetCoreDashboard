# AspNetCoreDashboard

ASP.NET Core **宿主包**，在 `net8.0` / `net9.0` 应用中挂载 `IUiModule`：嵌入静态文件、API 路由、SPA 回退、授权与健康检查。

| 属性 | 值 |
|------|-----|
| NuGet | `AspNetCoreDashboard` |
| TFM | `net8.0`、`net9.0` |
| 依赖 | `AspNetCoreDashboard.Abstractions` |

## 功能概览

- **DI 注册**：`AddUiModules()`、`AddModule<T>()`、`AddModulesFromAssembly()`
- **中间件管道**：授权 → `UseStaticFiles`（嵌入资源）→ API 路由 → SPA fallback
- **授权**：`IUiAuthorizationFilter` + ASP.NET Core **Policy**（`SetAuthorizationPolicy` / `WithAuthorization("PolicyName")`）
- **宿主选项**：`AddUiModuleHosting`（安全头、请求日志、请求体大小限制）
- **健康检查**：`AddUiModulesHealthCheck()`，返回各模块路由与嵌入 UI 元数据
- **可观测性**：`UiModuleActivitySource` 分布式追踪
- **静态资源**：流式输出、ETag/304、Range 请求支持

## 使用方法

### 最小示例

```csharp
using AspNetCoreDashboard.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUiModuleHosting(o => o.ApplySecurityHeaders = true);

builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());

var app = builder.Build();
app.UseUiModules();
app.Run();
```

### 基于 Policy 的授权

```csharp
builder.Services.AddAuthorization(o =>
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));

builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorizationPolicy("/MyModule", "AdminOnly");
```

### 健康检查

```csharp
builder.Services.AddHealthChecks().AddUiModulesHealthCheck();
app.MapHealthChecks("/health/modules");
```

### 程序集扫描

```csharp
builder.Services.AddUiModules()
    .AddModulesFromAssembly(typeof(MyUiModule).Assembly, builder.Services);
```

## 请求处理顺序

```
HTTP /{PathPrefix}/...
  → UiModuleAuthorizationMiddleware
  → UseStaticFiles（EmbeddedFileProvider）
  → AspNetCoreUiModuleMiddleware（API 路由 / SPA fallback）
```

## 主要扩展方法

| 扩展 | 命名空间 | 说明 |
|------|----------|------|
| `AddUiModules` | `Microsoft.Extensions.DependencyInjection` | 注册 `UiModuleRegistry` 与挂载跟踪器 |
| `AddUiModuleHosting` | `Microsoft.Extensions.DependencyInjection` | 全局宿主选项 |
| `AddUiModulesHealthCheck` | `Microsoft.Extensions.DependencyInjection` | 模块健康检查 |
| `UseUiModules` | `Microsoft.AspNetCore.Builder` | 挂载所有已注册模块 |
| `UseUiModule<T>` | `Microsoft.AspNetCore.Builder` | 挂载单个模块 |
| `WithAuthorization` | `AspNetCoreDashboard.Extensions` | 模块 `Configure` 内声明授权 |

## 注意事项

- 必须在 `builder.Build()` **之前**完成 `AddUiModules` 与模块注册；在 `app` 上调用 `UseUiModules()`。
- 反向代理场景需在 `UseUiModules()` 之前配置 `UseForwardedHeaders()`（见 [安全指南](../../docs/SECURITY.md)）。
- `SetAuthorizationPolicy` 仅在本宿主包可用；OWIN 请使用 `AspNetCoreDashboard.Owin` 的适配器。
- 同一 `PathPrefix` 重复挂载会由 `IUiModuleMountTracker` 抛出异常。
- 模块作者项目只需引用 `Abstractions`；**宿主应用**引用本包。

## 相关文档

- [模块作者指南](../../docs/MODULE_AUTHOR_GUIDE.md)
- [实用食谱](../../docs/COOKBOOK.md)
- [示例：WebSamples](../../Samples/AspNetCoreDashboardWebSamples)
