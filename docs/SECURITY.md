# 安全指南

## 默认启用授权

嵌入式 UI 模块常暴露管理类 API。**生产环境切勿在无授权的情况下挂载模块。**

推荐默认配置：

```csharp
// ASP.NET Core 8
builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());

// 或使用 ASP.NET Core 授权策略
builder.WithAuthorization("AdminOnly");
```

```csharp
// OWIN / net48
app.AddUiModules()
   .AddModule<MyUiModule>()
   .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());
app.UseUiModules();
```

## 反向代理与 `LocalRequestsOnlyAuthorizationFilter`

`LocalRequestsOnlyAuthorizationFilter` 将 `RemoteIpAddress` 与回环地址及服务器本地 IP 比较。在反向代理（nginx、IIS ARR、Azure App Gateway、Kubernetes Ingress）之后：

- 应用可能将**代理 IP** 视为远程地址，而非原始客户端。
- **不会**自动信任 `X-Forwarded-For`。

### ASP.NET Core 8

在 `UseUiModules()` **之前**启用转发头：

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    // 生产环境请将 KnownProxies / KnownNetworks 限制为负载均衡器地址。
});

var app = builder.Build();
app.UseForwardedHeaders();
app.UseUiModules();
```

生产环境请优先使用明确的授权**策略**（已认证用户、角色或自定义 `IUiAuthorizationFilter`），而非仅依赖 IP 检查。

### OWIN / net48

实现自定义 `IUiAuthorizationFilter` 读取并校验受信任的代理头，或在 IIS / 反向代理层强制认证。

## CSRF 与变更类 API

`MapPost`、`MapPut`、`MapDelete` 处理器接受请求体，但不内置防伪令牌。面向浏览器的表单建议：

- 使用 SameSite Cookie 与已认证会话，或
- 在处理器中校验防伪令牌，或
- 将变更类端点限制为非浏览器客户端（API Key、Bearer Token）。

## 路径前缀隔离

每个模块在唯一的 `PathPrefix` 下注册。重复前缀会在 `UiModuleRegistry` 注册时抛出异常。
