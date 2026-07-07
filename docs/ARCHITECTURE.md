# 架构说明

## 包结构

| 包 | 目标框架 | 职责 |
|----|----------|------|
| `AspNetCoreDashboard.Abstractions` | netstandard2.0 | `IUiModule`、`IUiContext`、`UiModuleRegistry` 等共享契约 |
| `AspNetCoreDashboard` | net8.0 | ASP.NET Core 宿主（`AddUiModules` / `UseUiModules`、静态文件、Policy 授权） |
| `AspNetCoreDashboard.Owin` | net48 | .NET Framework 应用的 OWIN 宿主 |
| `AspNetCoreDashboard.Testing` | net8.0 | 集成测试辅助（`UiModuleWebApplicationFactory`） |

## 请求流程（ASP.NET Core）

```
HTTP 请求 /{PathPrefix}/...
        │
        ▼
UiModuleAuthorizationMiddleware（授权）
        │
        ▼
UseStaticFiles（EmbeddedFileProvider 嵌入静态资源）
        │
        ▼
AspNetCoreUiModuleMiddleware
   ├─ 匹配 MapGet / MapPost / MapPut / MapDelete 处理器
   └─ 未匹配时回退到 index.html
```

## 请求流程（OWIN）

```
HTTP 请求 /{PathPrefix}/...
        │
        ▼
OwinUiModuleMiddleware
   ├─ IUiAuthorizationFilter 授权
   ├─ 匹配 MapGet / MapPost / MapPut / MapDelete 处理器
   ├─ 读取嵌入静态资源
   └─ 未匹配时回退到 index.html
```

## net8 与 net48 行为差异

| 场景 | net8.0 | net48 OWIN |
|------|--------|------------|
| 静态文件 | `EmbeddedFileProvider` + `UseStaticFiles` | 中间件内直接读嵌入流 |
| 中间件结构 | 授权 → 静态文件 → 路由（三段） | 单中间件内顺序处理 |
| SPA fallback | 未匹配且路径非 `.html` 时返回 index | 先尝试精确资源路径，再 fallback index |
| 静态缓存 | `index.html` → `no-cache`；其他资源 → `WithStaticCache` | 同左 |
| 授权 401/403 | 已认证 → 403，否则 401 | 同左 |
| DI 注册 | `IServiceCollection.AddUiModules()` | `IAppBuilder.AddUiModules()` |

模块作者只需实现 `IUiModule`；上述差异由宿主包封装，一般无需分支代码。

## net8 与 net48 能力对比

| 能力 | net8.0 | net48 OWIN |
|------|--------|------------|
| API 路由 | 有序 `UiRouteTable` | 有序 `OwinUiRouteTable` |
| HTTP 方法 | GET / POST / PUT / DELETE | GET / POST / PUT / DELETE |
| 授权 | `IUiAuthorizationFilter` + 可选 Policy | `IUiAuthorizationFilter` |
| 路由模板 | `/api/items/{id}` | `/api/items/{id}` |

## 模块作者的依赖规则

- UI 类库**仅引用** `AspNetCoreDashboard.Abstractions`
- 宿主应用按平台引用 `AspNetCoreDashboard` 或 `AspNetCoreDashboard.Owin`
- 两边宿主使用相同的 `UiModuleRegistry` 注册模式：`AddUiModules()` → `AddModule<T>()` → `SetAuthorization(...)` → `UseUiModules()`
- `PathPrefix` 不可重复注册

## 示例项目关系

```
LibrarySamples（SampleUiModule，netstandard2.0）
        │
        ├── WebSamples（net8 宿主，引用 AspNetCoreDashboard）
        └── OwinSamples（net48 宿主，引用 AspNetCoreDashboard.Owin）
```

同一套 `IUiModule` 可在两种宿主下复用，无需修改模块代码。
