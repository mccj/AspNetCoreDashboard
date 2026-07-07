# 从 v2.x 迁移到 v3.x

AspNetCoreDashboard 3.0 移除了 Hangfire 时代的遗留 API。本文说明破坏性变更与推荐替代方案。

## 已移除 API

| v2.x | v3.x 替代 |
|------|-----------|
| `UseMapDashboard()` | `AddUiModules()` + `UseUiModules()` 配合 `IUiModule` |
| `Compatibility/*` 类型 | 删除引用；实现 `IUiModule` |
| OWIN `LegacyOwin*` 仪表板 | 在 `IAppBuilder` 上使用 `AddUiModules()` + `UseUiModules()` |

## 模块注册

**之前（v2 兼容模式）：**

```csharp
app.UseMapDashboard(options => { /* Hangfire 风格路由 */ });
```

**之后（v3）：**

```csharp
builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());
app.UseUiModules();
```

## 路由

v3 在 `IUiModule.Configure()` 中使用有序路由：

```csharp
builder.MapGet("/api/items/{id}", ctx => ctx.WriteAsync(ctx.GetRouteValue("id")));
```

通配符如 `/.*` 仍可用。命名参数请优先使用 `{id}` 模板。

## 授权

行为不变：未认证客户端收到 **401**；已认证但被过滤器拒绝的客户端收到 **403**。

## OWIN（.NET Framework 4.8）

```csharp
app.AddUiModules()
   .AddModule<MyUiModule>()
   .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());
app.UseUiModules();
```

## v3.1 新增

- `AddModulesFromAssembly(assembly)` 程序集扫描
- `IUiModuleMountTracker`（DI 单例）替代静态挂载跟踪
- `MapPatch` / `MapOptions`、`IUiContext.RequestAborted`、`WriteStreamAsync`
- 分析器 ACD002–ACD004：嵌入资源与重复路由

## 需要帮助？

请参阅 [MODULE_AUTHOR_GUIDE.md](MODULE_AUTHOR_GUIDE.md) 与 [ARCHITECTURE.md](ARCHITECTURE.md)。
