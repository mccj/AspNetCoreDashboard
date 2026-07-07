# AspNetCoreDashboard.Abstractions

面向 UI 模块作者的**共享契约包**。定义 `IUiModule`、路由表、请求上下文、授权筛选器与嵌入资源工具，供各宿主（ASP.NET Core、OWIN、System.Web）复用。

| 属性 | 值 |
|------|-----|
| NuGet | `AspNetCoreDashboard.Abstractions` |
| TFM | `netstandard2.0` |
| 角色 | UI 模块类库**唯一必需**的宿主无关依赖 |

## 功能概览

- **`IUiModule`**：模块入口，声明 `PathPrefix` 并在 `Configure` 中注册路由与静态资源
- **`IUiModuleRegistration`**：流式 API（`MapGet`、`MapEmbeddedUi`、`MapFallbackToIndex` 等）
- **`IUiContext`**：与宿主无关的请求/响应抽象（查询、表单、JSON、上传、Cookie、路由参数）
- **`UiRouteTable` / `UiRoutePattern`**：有序路由匹配，支持 `{id}`、`{id:int}`、`{*path}` 等模板
- **授权**：`IUiAuthorizationFilter` 及内置 `LocalRequestsOnlyAuthorizationFilter`、`RequireRoleAuthorizationFilter`、`TrustedForwardedHeadersAuthorizationFilter`
- **嵌入资源**：`EmbeddedResourceCache`、`EmbeddedResourceEtag`、`EmbeddedResourcePathHelper`
- **诊断**：`UiModuleStartupValidator`、`UiModuleDiagnostics`、`UiModuleActivitySource`（OpenTelemetry）
- **MSBuild 目标**：`build/AspNetCoreDashboard.EmbeddedContent.targets` 自动嵌入 `Content/**`

## 使用方法

### 1. 引用包

```xml
<PackageReference Include="AspNetCoreDashboard.Abstractions" Version="3.6.0" />
```

### 2. 实现模块

```csharp
[UiModule("/MyModule", ContentNamespace = "MyCompany.MyModule.Content")]
public sealed class MyUiModule : IUiModule
{
    public string PathPrefix => "/MyModule";

    public void Configure(IUiModuleRegistration builder)
    {
        builder.MapEmbeddedUi(typeof(MyUiModule).Assembly, "MyCompany.MyModule.Content")
               .MapFallbackToIndex("MyCompany.MyModule.Content.index.html")
               .MapGet("/api/status", ctx => ctx.WriteJsonAsync(new { ok = true }));
    }
}
```

### 3. 自动嵌入静态文件（推荐）

在 `.csproj` 中导入：

```xml
<Import Project="$(PkgAspNetCoreDashboard_Abstractions)/build/AspNetCoreDashboard.EmbeddedContent.targets"
        Condition="'$(PkgAspNetCoreDashboard_Abstractions)' != ''" />
```

默认将 `Content/**` 嵌入为 `{AssemblyName}.Content.*` 逻辑名。

### 4. 由宿主挂载

本包**不包含** HTTP 宿主。宿主应用需额外引用：

- ASP.NET Core → `AspNetCoreDashboard`
- OWIN / net48 → `AspNetCoreDashboard.Owin`

详见仓库根目录 [模块作者指南](../../docs/MODULE_AUTHOR_GUIDE.md)。

## 主要类型

| 类型 | 说明 |
|------|------|
| `UiModuleRegistry` | 模块与按路径授权的注册表（宿主侧使用） |
| `UiModuleAttribute` | 声明路径前缀与内容命名空间元数据 |
| `UiSecurityHeaders` | 管理类 UI 推荐的安全响应头 |
| `UiCookieSameSite` | Cookie `SameSite` 枚举 |

## 注意事项

- UI 模块类库**只应引用本包**，不要直接引用 ASP.NET Core 或 OWIN 宿主包，以保持跨平台复用。
- `PathPrefix` 必须以 `/` 开头，且在同一宿主内不可重复。
- `MapFallbackToIndex` / `MapSpaFallback` 的资源名必须与 MSBuild `LogicalName` 完全一致。
- `LocalRequestsOnlyAuthorizationFilter` 在反向代理后可能失效，生产环境请使用角色或 Policy 授权（见 [安全指南](../../docs/SECURITY.md)）。
- 本包启用 Public API 分析器（`RS0026`），公开 API 变更需更新 `PublicAPI.Shipped.txt`。

## 相关文档

- [架构说明](../../docs/ARCHITECTURE.md)
- [故障排查](../../docs/TROUBLESHOOTING.md)
