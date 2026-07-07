# 故障排查

## 嵌入静态文件返回 404

1. 运行时列出清单资源名：`assembly.GetManifestResourceNames()`。
2. 确认 `MapEmbeddedUi(assembly, "YourAssembly.Content")` 与 MSBuild `LogicalName` 一致。
3. 导入 `AspNetCoreDashboard.EmbeddedContent.targets`，或手动核对 `EmbeddedResource` 名称。
4. 使用 `EmbeddedResourcePathHelper.ToResourceName(baseNamespace, "css/site.css")` 计算期望的资源名。

## 分析器 ACD002：找不到回退资源

导入嵌入内容目标以生成 `.dashboard.embeddedresources` 清单：

```xml
<Import Project="path/to/AspNetCoreDashboard.EmbeddedContent.targets" />
```

确保 `MapFallbackToIndex("YourAssembly.Content.index.html")` 与逻辑资源名完全一致。

## 重复 PathPrefix / 挂载错误

`IUiModuleMountTracker` 会在两个模块共享同一 `PathPrefix` 时抛出异常。每个模块必须使用唯一前缀（例如 `/Dashboard`、`/Admin`）。

## 401 与 403

| 状态码 | 含义 |
|--------|------|
| **401** | 用户未认证 |
| **403** | 用户已认证，但授权过滤器/策略拒绝访问 |

## 反向代理后 `LocalRequestsOnlyAuthorizationFilter` 失效

应用看到的是代理 IP，而非客户端 IP。请使用 `ForwardedHeaders`（ASP.NET Core）或 `TrustedForwardedHeadersAuthorizationFilter`，或改用角色/策略授权。详见 [SECURITY.md](SECURITY.md)。

## OWIN 集成测试端口冲突

OWIN 测试绑定动态回环端口。若测试挂起，检查是否有上次运行遗留的 `HttpListener` 进程。

## 模块项目中的 Nullable / 分析器警告

将 `AspNetCoreDashboard.Analyzers` 作为开发依赖引用：

```xml
<PackageReference Include="AspNetCoreDashboard.Analyzers" Version="3.6.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

## System.Web 模块不处理请求

`AspNetCoreDashboard.SystemWeb` 为实验性实现。在 `Application_Start` 中通过 `UiModuleHttpModuleRegistry` 注册模块，并在 `web.config` 中添加 HttpModule。生产环境请优先使用 OWIN 或 ASP.NET Core 宿主。
