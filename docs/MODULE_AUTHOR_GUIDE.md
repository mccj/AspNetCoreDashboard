# 模块作者指南

本文说明如何编写可嵌入的 UI 模块，供宿主应用一行代码挂载。

## 1. 创建 UI 模块

引用 `AspNetCoreDashboard.Abstractions`，实现 `IUiModule`：

```csharp
public sealed class MyUiModule : IUiModule
{
    public string PathPrefix => "/MyModule";

    public void Configure(IUiModuleRegistration builder)
    {
        var assembly = typeof(MyUiModule).Assembly;

        builder.MapEmbeddedUi(assembly, "MyModule.Content")
               .MapFallbackToIndex("MyModule.Content.index.html")
               .MapGet("/api/status", async ctx =>
               {
                   await ctx.WriteJsonAsync(new { ok = true });
               });
    }
}
```

## 2. 嵌入静态文件

将 `Content/` 目录下的文件设为 **嵌入的资源**。资源命名需遵循：

```
{AssemblyTitle}.Content.index.html
{AssemblyTitle}.Content.css.site.css
{AssemblyTitle}.Content.js.app.js
```

### MSBuild 自动嵌入（推荐）

在 UI 模块 `.csproj` 中导入 Abstractions 包附带的目标文件：

```xml
<Import Project="$(PkgAspNetCoreDashboard_Abstractions)/build/AspNetCoreDashboard.EmbeddedContent.targets"
        Condition="'$(PkgAspNetCoreDashboard_Abstractions)' != ''" />
```

或复制仓库中的 `build/AspNetCoreDashboard.EmbeddedContent.targets`。默认将 `Content/**` 嵌入为 `{AssemblyName}.Content.*`。

调试资源找不到时，可使用 `EmbeddedResourcePathHelper.ToResourceName(baseNamespace, relativePath)` 核对完整资源名。

## 3. 在 ASP.NET Core 8 宿主中挂载

```csharp
builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());

var app = builder.Build();
app.UseUiModules();
```

### 基于 Policy 的授权（仅 ASP.NET Core）

```csharp
builder.Services.AddAuthorization(o =>
    o.AddPolicy("Dashboard", p => p.RequireAuthenticatedUser()));

// 在模块 Configure 中
builder.MapEmbeddedUi(...)
       .WithAuthorization("Dashboard");
```

### 基于 Filter 的授权

Filter 可在三处配置，语法两边一致：

```csharp
// 1. 注册时按 PathPrefix 设置（推荐）
registry.SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());

// 2. 挂载单个模块时传入
app.UseUiModule<MyUiModule>(new LocalRequestsOnlyAuthorizationFilter());

// 3. 模块 Configure 中声明
builder.MapEmbeddedUi(...).WithAuthorization(new LocalRequestsOnlyAuthorizationFilter());
```

`LocalRequestsOnlyAuthorizationFilter` 仅允许本机或本地 IP 访问，适合管理面板场景。

## 4. 在 OWIN（.NET Framework 4.8）宿主中挂载

```csharp
public void Configuration(IAppBuilder app)
{
    app.AddUiModules()
       .AddModule<MyUiModule>()
       .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());

    app.UseUiModules();
}
```

OWIN 不支持 ASP.NET Core Policy；模块内请使用 `WithAuthorization(IUiAuthorizationFilter)`。

## 5. 注册 API 端点

| 方法 | 说明 |
|------|------|
| `MapGet(pattern, handler)` | 注册 GET 请求处理器 |
| `MapPost(pattern, handler)` | 注册 POST 请求处理器 |
| `MapPut(pattern, handler)` | 注册 PUT 请求处理器 |
| `MapDelete(pattern, handler)` | 注册 DELETE 请求处理器 |
| `MapFallbackToIndex(resourceName)` | SPA 场景：未匹配路由时返回 index.html |
| `WithStaticCache(maxAgeSeconds)` | 静态资源长缓存（index.html 始终 no-cache） |
| `WithContentSecurityPolicy(policy)` | 设置模块级 Content-Security-Policy 响应头 |
| `WithMaxUploadBytes(bytes)` | 限制本模块请求体最大字节数 |

路径相对于模块的 `PathPrefix`。支持 **路由模板** 与 **遗留正则**：

```csharp
.MapGet("/api/items/{id}", ctx => ctx.WriteAsync(ctx.GetRouteValue("id")))
.MapGet("/.*", ...) // 遗留正则模式
```

### 请求上下文 `IUiContext`

| 成员 | 说明 |
|------|------|
| `GetQueryAsync` / `GetFormValueAsync` | 读取查询字符串与表单 |
| `ReadBodyAsStringAsync` / `ReadJsonAsync<T>` | 读取 JSON 请求体 |
| `OpenRequestBodyAsync` | 读取原始流（上传） |
| `GetFormFileAsync` | 读取 multipart 上传文件 |
| `GetRequestHeader` / `SetResponseHeader` | 请求/响应头 |
| `GetRouteValue(name)` | 读取 `{id}` 路由参数 |
| `SetCookie` / `GetRequestCookie` | 读写 Cookie |
| `StatusCode` | 设置 HTTP 状态码 |
| `WriteAsync` / `WriteJsonAsync` / `WriteStreamAsync` | 写入响应 |

## 6. Analyzers（可选）

引用 `AspNetCoreDashboard.Analyzers` 可在编译期检测常见问题（ACD001–ACD007），例如无效 `PathPrefix`、重复路由、SPA 回退配置错误等。

## 7. 安全

生产环境务必配置授权。反向代理场景见 [SECURITY.md](SECURITY.md)。

## 8. 参考示例

本仓库 `Samples/AspNetCoreDashboardLibrarySamples/SampleUiModule.cs` 提供了完整示例，包含：

- 嵌入 `index.html` 与静态资源
- `GET/POST /FlowStatistics` 命令端点
- REST 风格 `MapGet` / `MapPut` / `MapPatch` / `MapDelete`
- 文件上传与流式下载

更多场景见 [COOKBOOK.md](COOKBOOK.md)，故障排查见 [TROUBLESHOOTING.md](TROUBLESHOOTING.md)。
