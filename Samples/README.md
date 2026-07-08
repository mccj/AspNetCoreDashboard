# 示例项目

本目录演示如何在不同宿主中挂载嵌入式 UI 模块。

| 项目 | 说明 |
|------|------|
| `AspNetCoreDashboardLibrarySamples` | netstandard2.0 UI 模块（`SampleUiModule` + `DiagnosticsUiModule`） |
| `AspNetCoreDashboardWebSamples` | ASP.NET Core 最小宿主（net8/9/10） |
| `AspNetCoreDashboardOwinSamples` | OWIN 自宿主（.NET Framework 4.8），端口 1101 |

## 运行 Web 示例

```bash
dotnet run --project Samples/AspNetCoreDashboardWebSamples
```

打开：

- 仪表板：<http://localhost:5000/Dashboard/>
- 诊断模块：<http://localhost:5000/Diagnostics/api/status>
- 宿主健康检查：<http://localhost:5000/health>

## 运行 OWIN 示例

```bash
dotnet run --project Samples/AspNetCoreDashboardOwinSamples
```

打开 <http://localhost:1101/Dashboard/>（根路径 `/` 会重定向到仪表板）。

## Docker

```bash
docker build -f docker/Dockerfile -t aspnetcoredashboard-sample .
docker run -p 8080:8080 aspnetcoredashboard-sample
```

## 示例页面功能

`/Dashboard/` 提供交互式演示：

- 嵌入静态资源（css / js / 图片 / 字体）
- JSON API（`WriteJsonAsync`）
- REST 动词（GET / PUT / PATCH / DELETE / OPTIONS）
- 表单与 query 参数
- 文件上传与下载
- 迷你任务监控（内存 CRUD + CSV 导出）
- SPA history 回退（`/Dashboard/about`）

`DiagnosticsUiModule` 演示同一程序集内注册多个模块，并通过 `AddModulesFromAssembly` 自动发现。

## 集成测试环境

`AspNetCoreDashboardWebSamples` 的 `WebSampleHostConfiguration` 支持以下 `ASPNETCORE_ENVIRONMENT` 值，供集成测试使用：

| 环境 | 行为 |
|------|------|
| `Testing` | 无授权限制 |
| `Unauthorized` | 全部拒绝（401） |
| `Forbidden` | 已认证但拒绝（403） |
| `PolicyForbidden` | 缺少角色（403） |
| `AspNetCorePolicyForbidden` | ASP.NET Core Policy（403） |
