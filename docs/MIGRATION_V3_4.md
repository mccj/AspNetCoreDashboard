# v3.3 → v3.4 / v3.5 迁移指南

## v3.4

### `[UiModule]` 特性

```csharp
[UiModule("/Dashboard", ContentNamespace = "MyCompany.Dashboard.Content")]
public sealed class DashboardUiModule : IUiModule
{
    public string PathPrefix => "/Dashboard";
    // ...
}
```

生成器会读取特性值生成 `GeneratedPathPrefix` / `GeneratedContentNamespace`。

### Cookie `Secure` / `SameSite`

```csharp
context.SetCookie("token", value, secure: true, sameSite: UiCookieSameSite.Lax);
```

### 健康检查

```csharp
builder.Services.AddHealthChecks().AddUiModulesHealthCheck();
app.MapHealthChecks("/health/modules");
```

### 宿主选项

```csharp
builder.Services.AddUiModuleHosting(o =>
{
    o.ApplySecurityHeaders = true;
    o.EnableRequestLogging = true;
});
```

## v3.5

### 按模块 CSP 与上传限制

```csharp
builder.MapEmbeddedUi(assembly, "MyModule.Content")
       .WithContentSecurityPolicy("default-src 'self'")
       .WithMaxUploadBytes(10 * 1024 * 1024);
```

### OWIN 策略授权

```csharp
app.UseOwinAuthorizationAdapter(new MyPolicyAdapter());
registry.SetAuthorizationPolicy("/Dashboard", "AdminOnly");
app.UseUiModules();
```

### 可观测性

UI 模块请求会发出 `ActivitySource`：`AspNetCoreDashboard.UiModules`。健康检查响应包含每个模块的路由数与嵌入 UI 状态。
