# 变更日志

## [3.6.0]

### 新增

- ASP.NET Core `EmbeddedStaticFileMiddleware`：嵌入式静态资源流式输出，替代 `UseStaticFiles` + 全量缓冲
- OWIN `OwinUiModulePipelineMiddleware`：ActivitySource、按模块 CSP、`Content-Length` 上传限制（413）
- `MapUiModulesHealthChecks()`：健康检查端点输出含模块明细的 JSON
- 集成测试：`/health/modules`、上传大小限制、`IOwinAuthorizationAdapter` 策略授权
- `docs/MIGRATION_V3_5.md`

### 变更

- `UiRouteTable` 按 HTTP 方法分组匹配，减少无关路由扫描
- System.Web 静态资源与 API 回退改为 `OpenReadStream` 流式复制
- Public API：v3.4/v3.5 新增成员并入 `PublicAPI.Shipped.txt`
- 包验证基线升至 3.6.0
- 统一 C# 代码格式（2 空格缩进，与 `.editorconfig` 一致）

## [3.5.0]

### 新增

- `UiModuleActivitySource`：OpenTelemetry 分布式追踪
- `UiModuleDiagnostics` / `UiModuleSummary`：模块检查与健康检查明细
- `IOwinAuthorizationAdapter` 与 `UseOwinAuthorizationAdapter()`：OWIN 策略授权桥接
- `IUiModuleRegistration` 上的 `WithContentSecurityPolicy()` 与 `WithMaxUploadBytes()`
- `EmbeddedResourceEtag.Compute(Stream)`：基于流的 ETag 计算
- 分析器 **ACD007**（带 `[UiModule]` 但未调用 `MapEmbeddedUi`）
- ACD006 CodeFix（将 `PathPrefix` 与 `UiModuleAttribute` 对齐）
- Generator 与 System.Web 测试项目
- `docs/MIGRATION_V3_4.md`

### 变更

- 静态文件响应使用 `OpenReadStream`，支持 `Accept-Ranges` / Range（ASP.NET Core、OWIN）
- `UiRouteTable` 使用编译后的正则进行路由匹配
- `UiModulesHealthCheck` 返回各模块的路由与嵌入 UI 元数据
- 请求日志包含模块 `PathPrefix`
- Public API 基线由 `RS0016` 强制校验
- CI：net9 集成测试；Windows 构建收集覆盖率
- System.Web 包描述更新（不再标注为实验性骨架）

### 移除

- FileManager 集成指南（`docs/FILEMANAGER_INTEGRATION.md`）

## [3.4.0]

### 新增

- `[UiModule]` 特性：路径前缀与内容命名空间元数据
- `UiModuleStartupValidator`：启动时模块校验
- `UiModuleHostingOptions`：可选安全响应头与请求日志中间件
- `UiModulesHealthCheck` 与 `AddUiModulesHealthCheck()`
- `UiCookieSameSite` 与扩展的 `SetCookie`（`Secure`、`SameSite`）
- `EmbeddedResourceCache.OpenReadStream()`：大文件绕过内存缓存
- 分析器 **ACD006**（`PathPrefix` 与 `UiModuleAttribute` 不一致）
- 分析器测试 ACD003–ACD004、ACD006
- Testing 包中的 `UiModuleTestAssertions` 辅助方法
- System.Web：通过 `SystemWebUiContext` 完整执行 API 路由
- MSBuild 嵌入内容 `LogicalName` 校验目标
- 源生成器读取 `[UiModule]` 特性值

### 变更

- OWIN：对 `SetAuthorizationPolicy` 返回明确错误（请使用角色过滤器）
- `publish.yml` 打包 Generators 与 SystemWeb；发布前运行测试
- CI：`dotnet format --verify-no-changes`

### 测试

- OWIN 多部分上传集成测试
- `UiModuleStartupValidator` 单元测试

## [3.3.0]

### 新增

- `IUiContext.GetRequestCookie`、`UiContextRouteExtensions.GetRouteValues()`
- `UiModuleRegistry.SetAuthorizationPolicy`：ASP.NET Core 授权策略
- `IUiModuleRegistration.ExcludeSpaFallbackExtensions`：SPA 回退扩展名调优
- `EmbeddedResourceEtag`、`UiSecurityHeaders.ApplyBaseline`
- `EmbeddedResourceCache` LRU 限制（`MaxEntries`、`MaxBytes`）
- 嵌入静态资源与 SPA 回退的 ETag / 304 支持
- OWIN：`GetFormFileAsync`（multipart）、`GetRequestCookie`、HEAD 响应体剥离、启用 Nullable
- 分析器 **ACD005**（SPA 回退但未 `MapEmbeddedUi`）、ACD001 CodeFix
- 源生成器输出 `GeneratedPathPrefix` 约定常量
- System.Web：模块注册 API 路由时对 API 路径返回 **501**
- `PublicAPI.Shipped.txt` 基线与 CI `RS0016` / `RS0026` 门禁（Abstractions）

### 测试

- 集成：HEAD、multipart 上传、ETag 304、安全头、ASP.NET Core 策略 403
- 分析器：ACD005

## [3.2.0]

### 新增

- `IUiContext`：`User`、`GetFormFileAsync`、`SetCookie`、`RedirectAsync`
- `IUiFormFile`、`TrustedForwardedHeadersAuthorizationFilter`、`RequireRoleAuthorizationFilter`
- `EmbeddedResourceCache`：嵌入静态资源预热
- `MapHead`、`MapSpaFallback`、路由约束（`{id:int}`、`{id:guid}`、`{*path}`）
- HEAD 请求复用 GET 处理器且不写入响应体
- OWIN `WithRequiredRole` 授权扩展
- `AspNetCoreDashboard.Generators`（内容命名空间源生成器）
- `AspNetCoreDashboard.Analyzers.Tests` 项目
- System.Web HttpModule 提供嵌入静态文件（实验性）
- Razor `MapRazorPage` 加载嵌入的 `{PageType}.html` 模板
- 文档：`COOKBOOK.md`、`TROUBLESHOOTING.md`；`docker/docker-compose.yml`
- ASP.NET Core 宿主多目标 **net8.0** 与 **net9.0**
- CI Linux 任务：net8 + 分析器测试

### 变更

- 示例模块演示 PATCH/OPTIONS/导出流与 `{id:int}` 路由
- 扩展集成测试（OWIN PUT/DELETE/401、Policy 403、Patch/Options/下载）

## [3.1.0]

### 新增

- 分析器 ACD002–ACD004（嵌入回退资源、Content 命名空间、重复路由）
- MSBuild 嵌入资源清单（ACD002）
- `AddModulesFromAssembly`、`UiModuleAssemblyExtensions.GetUiModuleTypes()`
- `IUiModuleMountTracker` DI 服务（替代静态挂载跟踪）
- `MapPatch` / `MapOptions`、`IUiContext.RequestAborted`、`WriteStreamAsync`
- 403 集成测试（`Forbidden` 环境）、OWIN 动态端口测试
- `AspNetCoreDashboard.SystemWeb` 骨架 HttpModule 包
- `docs/MIGRATION_V2_V3.md`、`docker/Dockerfile`、`dotnet new` UI 模块模板
- CI Coverlet 代码覆盖率、NuGet 包校验基线 3.0.0
- Abstractions 启用可空引用类型；Abstractions Public API 分析器

### 变更

- Razor 预览 `MapRazorPage` 注册 GET HTML 外壳，不再抛出异常
- 移除未使用的示例 CSS（`site1.css`、`site2.css`、`site3.css`）

## [3.0.0]

### 新增

- Abstractions 中共享的 `UiRouteTable`、`UiHttpMethod`、`UiRoutePattern`
- 路由模板：`/api/items/{id}` 与 `GetRouteValue(name)`
- `IUiContext`：`OpenRequestBodyAsync`、`SetResponseHeader`
- `UiModuleMountTracker`：重复挂载检测
- `AspNetCoreDashboard.Analyzers`（ACD001 PathPrefix 校验）
- `AspNetCoreDashboard.Razor` 预览包
- OWIN 集成测试、扩展 ASP.NET Core 集成测试
- 标签推送触发 NuGet 发布工作流、Dependabot、`global.json`、包图标
- 恢复 `docs/FILEMANAGER_INTEGRATION.md`

### 变更

- **破坏性：** 移除 `UseMapDashboard` 及全部 `Compatibility/` 遗留代码
- **破坏性：** 移除 OWIN `LegacyOwin*` 仪表板代码
- 示例项目：移除未使用的 Bootstrap/jQuery 资源与 WebSamples `wwwroot/`
- LibrarySamples 使用 `AspNetCoreDashboard.EmbeddedContent.targets`
- 版本升至 3.0.0；README 徽章改用 GitHub Actions

## [2.1.0]

- 扩展 `IUiContext`、`MapPut`/`MapDelete`、缓存策略、Testing 包、CI pack

## [2.0.0]

- 三包架构与 `IUiModule` API
