# 实用食谱

AspNetCoreDashboard 模块作者的常见场景与代码片段。

## 程序集扫描注册

```csharp
builder.Services.AddUiModules()
    .AddModulesFromAssembly(typeof(MyUiModule).Assembly, builder.Services);
app.UseUiModules();
```

## 反向代理 + 仅本地访问

```csharp
registry.SetAuthorization("/Dashboard",
    new TrustedForwardedHeadersAuthorizationFilter("10.0.0.0")); // 入口/代理 IP
```

生产环境务必限制 `KnownProxies`，管理类 UI 优先使用基于策略的授权。

## 基于角色的授权（OWIN）

```csharp
public void Configure(IUiModuleRegistration builder)
{
    builder.WithRequiredRole("Admin")
           .MapGet("/api/status", ctx => ctx.WriteAsync("ok"));
}
```

## 文件下载

```csharp
.MapGet("/api/export/report.csv", async ctx =>
{
    await using var stream = File.OpenRead("report.csv");
    await ctx.WriteStreamAsync(stream, "text/csv", "report.csv");
});
```

## SPA History 模式回退

```csharp
builder.MapEmbeddedUi(assembly, "MyModule.Content")
       .MapSpaFallback("MyModule.Content.index.html");
```

未匹配的路由（无文件扩展名）返回 `index.html`，并设置 `Cache-Control: no-cache`。

## 路由约束

```csharp
.MapGet("/api/items/{id:int}", ctx => ctx.WriteAsync(ctx.GetRouteValue("id")));
```

支持的约束：`int`、`guid`、捕获全部 `{*path}`。

## HEAD 请求

`HEAD` 会自动复用匹配的 `GET` 处理器，且不写入响应体。

## Docker 示例

```bash
docker compose -f docker/docker-compose.yml up --build
```

打开 http://localhost:8080/Dashboard/
