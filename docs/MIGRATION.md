# 从 1.0.0-beta-19 迁移到 1.0.0-beta-20

本文面向仍在使用 [NuGet 上的 1.0.0-beta-19](https://www.nuget.org/packages/AspNetCoreDashboard/1.0.0-beta-19)（2023-02 发布）的应用与 UI 模块。目标版本为 **1.0.0-beta-20**。

## 包与目标框架

**1.0.0-beta-19：**

- 单一包 `AspNetCoreDashboard`（netstandard2.0 / net46）
- 宿主依赖 `Microsoft.AspNetCore.Mvc.Core` 2.2.x
- 路由基于 `RouteCollection` + Dispatcher（`EmbeddedResourceDispatcher`、`CommandDispatcher` 等）

**1.0.0-beta-20：**

| 角色 | 包 | TFM |
|------|-----|-----|
| UI 模块 | `AspNetCoreDashboard.Abstractions` | netstandard2.0 |
| ASP.NET Core 宿主 | `AspNetCoreDashboard` | net8.0; net9.0; net10.0 |
| OWIN 宿主 | `AspNetCoreDashboard.Owin` | net48 |
| 可选：源生成器 | `AspNetCoreDashboard.Generators` | netstandard2.0 |
| 可选：分析器 | `AspNetCoreDashboard.Analyzers` | netstandard2.0 |

宿主应用需升级到 **.NET 8 或更高**（OWIN 场景为 **.NET Framework 4.8**）。模块项目可继续面向 netstandard2.0，仅引用 Abstractions。

```xml
<!-- 模块项目 -->
<PackageReference Include="AspNetCoreDashboard.Abstractions" Version="1.0.0-beta-20" />

<!-- ASP.NET Core 宿主 -->
<PackageReference Include="AspNetCoreDashboard" Version="1.0.0-beta-20" />

<!-- OWIN 宿主 -->
<PackageReference Include="AspNetCoreDashboard.Owin" Version="1.0.0-beta-20" />
```

## 已移除 API

| 1.0.0-beta-19 | 1.0.0-beta-20 替代 |
|---------------|-------------------|
| `UseMapDashboard(path, routes => …)` | `AddUiModules()` + `UseUiModules()` 配合 `IUiModule` |
| `RouteCollection` / `routes.Add(…)` | `IUiModuleRegistration.MapGet` / `MapPost` / … |
| `RouteCollectionExtensions.AddEmbeddedResource` | `MapEmbeddedUi` + `MapFallbackToIndex` / `MapSpaFallback` |
| `RouteCollectionExtensions.AddCommand` | `MapGet` / `MapPost` / `MapPut` / `MapDelete` 等 |
| `RouteCollectionExtensions.AddRazorPage` | `AspNetCoreDashboard.Razor` 的 `MapRazorPage`（预览），或嵌入 HTML |
| `EmbeddedResourceDispatcher`、`CommandDispatcher`、`RedirectDispatcher` 等 | 删除；在 `IUiModule.Configure` 中声明路由 |
| `IDashboardContext` / `DashboardRequest` / `DashboardResponse` | `IUiContext` |
| `IDashboardAuthorizationFilter` | `IUiAuthorizationFilter`（`LocalRequestsOnlyAuthorizationFilter` 仍可用） |

## 模块注册

**之前（beta-19）：**

```csharp
app.UseMapDashboard("/Dashboard", routes =>
{
    var assembly = typeof(MyDashboard).Assembly;
    var ns = "MyAssembly.Content";

    routes.AddEmbeddedResource(assembly, "/(?<path>.*)", string.Empty, ns);
    routes.AddCommand("/FlowStatistics", async context =>
    {
        var filter = context.Request.Method == "POST"
            ? await context.Request.GetFormValueAsync("filter")
            : context.Request.GetQuery("filter");
        await context.Response.WriteAsync("…");
        return true;
    });
});
```

**之后（beta-20）：**

实现 `IUiModule`，在宿主中注册：

```csharp
public sealed class MyUiModule : IUiModule
{
    public string PathPrefix => "/Dashboard";

    public void Configure(IUiModuleRegistration builder)
    {
        var assembly = typeof(MyUiModule).Assembly;

        builder.MapEmbeddedUi(assembly, "MyAssembly.Content")
               .MapFallbackToIndex("MyAssembly.Content.index.html")
               .MapGet("/FlowStatistics", HandleFlowStatisticsGet)
               .MapPost("/FlowStatistics", HandleFlowStatisticsPost);
    }

    private static Task HandleFlowStatisticsGet(IUiContext ctx) => …;
    private static Task HandleFlowStatisticsPost(IUiContext ctx) => …;
}

// ASP.NET Core 宿主
builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorization("/Dashboard", new LocalRequestsOnlyAuthorizationFilter());
app.UseUiModules();
```

嵌入资源命名约定仍为 `{AssemblyName}.Content.*`（例如 `MyAssembly.Content.index.html`）。可使用 Abstractions 包附带的 `AspNetCoreDashboard.EmbeddedContent.targets` 自动嵌入 `Content/` 目录。

## 路由

beta-19 使用正则模板与命名组，例如 `/(?<path>.*)`。beta-20 使用有序路由模板：

```csharp
builder.MapGet("/api/items/{id}", ctx => ctx.WriteAsync(ctx.GetRouteValue("id")));
```

通配符路由仍可用（如 `/{*path}`）。命名参数请优先使用 `{id}` 模板，支持 `{id:int}`、`{id:guid}` 等约束。

## 授权

行为不变：未认证客户端收到 **401**；已认证但被过滤器拒绝的客户端收到 **403**。

将 `IDashboardAuthorizationFilter` 实现替换为 `IUiAuthorizationFilter`，或在注册时按 `PathPrefix` 设置内置过滤器。

## OWIN（.NET Framework 4.8）

```csharp
app.AddUiModules()
   .AddModule<MyUiModule>()
   .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());
app.UseUiModules();
```

## beta-20 推荐能力

完成核心迁移后，以下能力相对 beta-19 均为新增，无额外破坏性变更。

### `[UiModule]` 特性

```csharp
[UiModule("/Dashboard", ContentNamespace = "MyCompany.Dashboard.Content")]
public sealed class DashboardUiModule : IUiModule
{
    public string PathPrefix => "/Dashboard";
    // ...
}
```

源生成器会读取特性值生成 `GeneratedPathPrefix` / `GeneratedContentNamespace`。引用 `AspNetCoreDashboard.Generators` 并启用分析器可捕获 `PathPrefix` 与特性不一致等问题（ACD006）。

### 路由与上下文扩展

- `AddModulesFromAssembly(assembly)` 程序集扫描
- `MapPatch` / `MapOptions`、`MapHead`、`MapSpaFallback`
- `IUiContext`：`RequestAborted`、`WriteStreamAsync`、`GetFormFileAsync`、`SetCookie`、`RedirectAsync`
- `UiModuleRegistry.SetAuthorizationPolicy`（ASP.NET Core 授权策略）
- `AspNetCoreDashboard.Generators` 源生成器、`AspNetCoreDashboard.Analyzers` 编译期检查
- `dotnet new acduimodule` UI 模块模板

### Cookie `Secure` / `SameSite`

```csharp
context.SetCookie("token", value, secure: true, sameSite: UiCookieSameSite.Lax);
```

### 健康检查

```csharp
builder.Services.AddHealthChecks().AddUiModulesHealthCheck();
app.MapHealthChecks("/health/modules");
```

也可使用 `MapUiModulesHealthChecks()` 输出含各模块路由数与嵌入 UI 状态的 JSON。

### 宿主选项

```csharp
builder.Services.AddUiModuleHosting(o =>
{
    o.ApplySecurityHeaders = true;
    o.EnableRequestLogging = true;
});
```

`ApplySecurityHeaders` 为嵌入 UI 响应应用基线安全头（CSP 除外，见按模块 CSP）。

### 按模块 CSP 与上传限制

```csharp
builder.MapEmbeddedUi(assembly, "MyModule.Content")
       .WithContentSecurityPolicy("default-src 'self'")
       .WithMaxUploadBytes(10 * 1024 * 1024);
```

OWIN 宿主通过管道中间件同样强制执行 CSP 与 `Content-Length` 上传上限（超限返回 413）。

### OWIN 策略授权

beta-19 仅支持基于 Filter 的授权。beta-20 起 OWIN 可通过策略适配器桥接现有授权体系：

```csharp
app.UseOwinAuthorizationAdapter(new MyPolicyAdapter());
registry.SetAuthorizationPolicy("/Dashboard", "AdminOnly");
app.UseUiModules();
```

ASP.NET Core 宿主使用 `SetAuthorizationPolicy` / `WithAuthorization("PolicyName")`。

### 可观测性

UI 模块请求会发出 OpenTelemetry `ActivitySource`：`AspNetCoreDashboard.UiModules`。启用 `AddUiModuleHosting` 的请求日志会包含模块 `PathPrefix`。

## 行为变更

### ASP.NET Core 嵌入式静态文件

beta-20 宿主通过专用 `EmbeddedStaticFileMiddleware` 流式提供模块内嵌 UI，不再经 `UseMapDashboard` 内部的 Dispatcher 全量缓冲响应体。

若你曾在 beta-19 中通过 `UseStaticFiles` 配合自定义 `IFileProvider` 为模块 UI 服务静态资源，请改为依赖 `MapEmbeddedUi`，或在模块路由之外单独配置 StaticFiles。

静态资源支持 ETag、`Accept-Ranges` / Range，以及 SPA 回退逻辑。

### OWIN 管道顺序

`UseUiModule` 在 `OwinUiModuleMiddleware` 之前挂载 `OwinUiModulePipelineMiddleware`，用于：

- OpenTelemetry `ActivitySource` 标签
- 按模块 `WithContentSecurityPolicy()` 响应头
- `WithMaxUploadBytes()` / 全局上传限制（`Content-Length` 超限返回 413）

无需改应用代码；若你自行 `Map` 分支并只挂载 `OwinUiModuleMiddleware`，建议同样在前面加上管道中间件（通过 `UseUiModule` 已自动完成）。

### System.Web 流式输出

大文件不再经 `GetBytes` 全量加载。API 与静态响应使用 `OpenReadStream`，内存占用更低。

### UiRouteTable

路由表内部按 `UiHttpMethod` 分桶。对外 API 不变；若你复制了 beta-19 时代 `RouteCollection` 的匹配逻辑，应改用 `IUiModuleRegistration` 的公开路由 API。

## 升级检查清单

1. 将所有 `AspNetCoreDashboard` 相关包统一到 **1.0.0-beta-20**
2. 确认已移除对 `UseMapDashboard`、`RouteCollection`、各类 `*Dispatcher` 的引用
3. 将 `IDashboardContext` / `IDashboardAuthorizationFilter` 替换为 `IUiContext` / `IUiAuthorizationFilter`
4. 重新编译模块项目，确认无 Public API 分析器（RS0016）新警告
5. 运行集成测试或手动验证嵌入 UI、API 路由、授权与健康检查端点
6. 按需启用宿主选项、健康检查与可观测性

## 配置示例（beta-20 完整宿主）

```csharp
builder.Services.AddUiModuleHosting(o =>
{
    o.ApplySecurityHeaders = true;
    o.EnableRequestLogging = true;
});
builder.Services.AddHealthChecks().AddUiModulesHealthCheck();
builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());

var app = builder.Build();
app.MapHealthChecks("/health/modules");
app.UseUiModules();
```

模块内：

```csharp
builder.MapEmbeddedUi(assembly, "MyModule.Content")
       .WithMaxUploadBytes(10 * 1024 * 1024)
       .WithContentSecurityPolicy("default-src 'self'");
```

OWIN 策略授权：

```csharp
app.UseOwinAuthorizationAdapter(new MyPolicyAdapter());
app.AddUiModules()
   .AddModule<MyUiModule>()
   .SetAuthorizationPolicy("/MyModule", "AdminOnly");
app.UseUiModules();
```

## 需要帮助？

请参阅 [MODULE_AUTHOR_GUIDE.md](MODULE_AUTHOR_GUIDE.md) 与 [ARCHITECTURE.md](ARCHITECTURE.md)。
