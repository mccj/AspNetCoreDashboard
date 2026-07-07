# v3.5 → v3.6 迁移指南

## 行为变更

### ASP.NET Core 嵌入式静态文件

宿主不再通过 `UseStaticFiles` 提供模块内嵌 UI。若你曾依赖 StaticFiles 中间件在模块分支上的副作用（例如自定义 `IFileProvider`），请改为在模块外单独配置。

静态资源仍支持 ETag、`Accept-Ranges` / Range，以及 SPA 回退逻辑。

### OWIN 管道顺序

`UseUiModule` 现在在 `OwinUiModuleMiddleware` 之前挂载 `OwinUiModulePipelineMiddleware`，用于：

- OpenTelemetry `ActivitySource` 标签
- 按模块 `WithContentSecurityPolicy()` 响应头
- `WithMaxUploadBytes()` / 全局上传限制（`Content-Length` 超限返回 413）

无需改应用代码；若你自行 `Map` 分支并只挂载 `OwinUiModuleMiddleware`，建议同样在前面加上管道中间件（通过 `UseUiModule` 已自动完成）。

### System.Web 流式输出

大文件不再经 `GetBytes` 全量加载。API 与静态响应使用 `OpenReadStream`，内存占用更低。

### UiRouteTable

路由表内部按 `UiHttpMethod` 分桶。对外 API 不变；自定义复制 `UiRouteTable` 逻辑的代码应改用公开 `Add` / `TryMatch`。

## 可选配置

```csharp
builder.MapEmbeddedUi(assembly, ns)
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

## Public API

3.6 将 v3.4/v3.5 未发布 API 正式纳入 `PublicAPI.Shipped.txt`。升级后请重新编译模块项目，确认无新的 Public API 分析器警告。
