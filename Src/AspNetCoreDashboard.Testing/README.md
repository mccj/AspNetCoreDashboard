# AspNetCoreDashboard.Testing

ASP.NET Core UI 模块 **集成测试辅助包**，简化内存宿主搭建与常见 HTTP 断言。

| 属性 | 值 |
|------|-----|
| NuGet | `AspNetCoreDashboard.Testing` |
| TFM | `net8.0`、`net9.0`、`net10.0` |
| 依赖 | `AspNetCoreDashboard`、`Microsoft.AspNetCore.Mvc.Testing` |
| 角色 | 测试项目可选依赖 |

## 功能概览

- **`UiModuleWebApplicationFactory<TEntryPoint>`**：基于 `WebApplicationFactory` 注册并挂载 UI 模块
- **`UiModuleTestAssertions`**：ETag、304、安全头、401、403 等断言辅助
- **`DenyAllAuthorizationFilter`**：测试拒绝所有请求的授权筛选器

## 使用方法

### 方式一：测试现有宿主入口点（推荐）

直接继承 `WebApplicationFactory<Program>`，在 `ConfigureWebHost` 中配置模块：

```csharp
public class DashboardTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DashboardTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Dashboard_returns_ok()
    {
        var response = await _client.GetAsync("/Dashboard/");
        response.EnsureSuccessStatusCode();
    }
}
```

### 方式二：使用 UiModuleWebApplicationFactory

无需真实 `Program` 时，由工厂自动 `AddUiModules` + `UseUiModules`：

```csharp
public class ModuleTests : IClassFixture<UiModuleWebApplicationFactory<Program>>
{
    public ModuleTests()
    {
        _factory = new UiModuleWebApplicationFactory<Program>((registry, services) =>
        {
            registry.AddModule<MyUiModule>(services)
                    .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());
        });
        _client = _factory.CreateClient();
    }
}
```

### 断言辅助

```csharp
using AspNetCoreDashboard.Testing;

UiModuleTestAssertions.AssertHasEtag(response);
UiModuleTestAssertions.AssertNotModified(response);
UiModuleTestAssertions.AssertSecurityHeaders(response);
UiModuleTestAssertions.AssertUnauthorized(response);
UiModuleTestAssertions.AssertForbidden(response);
```

### 模拟 401 / 403

配合测试环境与 `DenyAllAuthorizationFilter`：

```csharp
// 宿主 Program 中按环境切换授权
if (builder.Environment.IsEnvironment("Forbidden"))
{
    builder.Services.AddAuthentication("Test").AddScheme<...>("Test", null);
    registry.SetAuthorization("/Dashboard", new DenyAllAuthorizationFilter());
}
```

## 主要类型

| 类型 | 说明 |
|------|------|
| `UiModuleWebApplicationFactory<TEntryPoint>` | 内存宿主 + 模块注册回调 |
| `UiModuleTestAssertions` | 静态断言方法 |
| `DenyAllAuthorizationFilter` | 始终返回 `Authorize == false` |

## 注意事项

- 仅适用于 **ASP.NET Core** 集成测试；OWIN 测试需自宿主 + `HttpClient`（见 `AspNetCoreDashboard.Owin.Tests`）。
- `UiModuleWebApplicationFactory` 在回调中可调用 `services.AddUiModules()`；工厂内部也会创建注册表，避免重复注册。
- 测试授权行为时，使用 `builder.UseEnvironment("Forbidden")` 等自定义环境名，与示例 WebSamples 保持一致。
- 本包**不**替代 `Microsoft.AspNetCore.Mvc.Testing`；需同时引用 Testing 框架（xUnit 等）。

## 相关示例

- [AspNetCoreDashboard.Tests](../../Tests/AspNetCoreDashboard.Tests)
- [WebSamples 测试环境配置](../../Samples/AspNetCoreDashboardWebSamples/Program.cs)
