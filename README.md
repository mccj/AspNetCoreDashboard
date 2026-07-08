# AspNetCoreDashboard

面向 NuGet 类库作者的嵌入式 Web UI 宿主 SDK。可在 ASP.NET Core 8/9/10 或 OWIN（.NET Framework 4.8）应用中挂载自包含的 UI + API 模块。

[![Build](https://github.com/mccj/AspNetCoreDashboard/actions/workflows/build.yml/badge.svg)](https://github.com/mccj/AspNetCoreDashboard/actions/workflows/build.yml)
[![Build status](https://ci.appveyor.com/api/projects/status/ljllh9mfd0aaleoi?svg=true)](https://ci.appveyor.com/project/mccj/aspnetcoredashboard-dashboard)
[![MyGet](https://img.shields.io/myget/mccj/vpre/AspNetCoreDashboard.svg)](https://myget.org/feed/mccj/package/nuget/AspNetCoreDashboard)
[![NuGet](https://img.shields.io/nuget/v/AspNetCoreDashboard.svg)](https://www.nuget.org/packages/AspNetCoreDashboard)

## .NET 支持策略

| 宿主 | TFM | 支持状态 |
|------|-----|----------|
| ASP.NET Core | **net8.0**、**net9.0**、**net10.0** | 活跃维护 — 大版本发布时对齐当前 LTS / 当前版本 |
| OWIN / .NET Framework | **net48** | 活跃维护 — 缺陷修复，与 Abstractions API 对齐 |
| UI 模块编写 | **netstandard2.0** | 活跃维护 — 仅引用 Abstractions |
| System.Web | **net48** | 活跃维护 — API 路由与流式静态资源；复杂场景优先 OWIN 或 ASP.NET Core |

新版 SDK 大版本与 ASP.NET Core LTS 发布节奏对齐；同一主版本内的补丁发布保持向后兼容。

## 包说明

| 包 | TFM | 适用场景 |
|----|-----|----------|
| [AspNetCoreDashboard.Abstractions](Src/AspNetCoreDashboard.Abstractions/README.md) | netstandard2.0 | 编写 UI 模块 |
| [AspNetCoreDashboard](Src/AspNetCoreDashboard/README.md) | net8.0;net9.0;net10.0 | ASP.NET Core 宿主 |
| [AspNetCoreDashboard.Owin](Src/AspNetCoreDashboard.Owin/README.md) | net48 | OWIN / .NET Framework 宿主 |
| [AspNetCoreDashboard.Generators](Src/AspNetCoreDashboard.Generators/README.md) | netstandard2.0 | 内容命名空间与路径前缀源生成器 |
| [AspNetCoreDashboard.Testing](Src/AspNetCoreDashboard.Testing/README.md) | net8.0;net9.0;net10.0 | 集成测试辅助（可选） |
| [AspNetCoreDashboard.Analyzers](Src/AspNetCoreDashboard.Analyzers/README.md) | netstandard2.0 | 编译期模块检查（可选） |
| [AspNetCoreDashboard.Razor](Src/AspNetCoreDashboard.Razor/README.md) | net8.0;net9.0;net10.0 | Razor 预览路由（可选） |
| [AspNetCoreDashboard.SystemWeb](Src/AspNetCoreDashboard.SystemWeb/README.md) | net48 | System.Web HttpModule 适配器 |

## 快速开始（ASP.NET Core 8 / 9 / 10）

```csharp
[UiModule("/MyModule")]
public sealed class MyUiModule : IUiModule
{
    public string PathPrefix => "/MyModule";

    public void Configure(IUiModuleRegistration builder)
    {
        builder.MapEmbeddedUi(typeof(MyUiModule).Assembly, "MyModule.Content")
               .MapFallbackToIndex("MyModule.Content.index.html")
               .MapGet("/api/items/{id}", ctx => ctx.WriteAsync(ctx.GetRouteValue("id")));
    }
}

builder.Services.AddUiModuleHosting(o => o.ApplySecurityHeaders = true);
builder.Services.AddUiModules()
    .AddModule<MyUiModule>(builder.Services)
    .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());
app.UseUiModules();
```

使用 `dotnet new install ./templates` 安装模板后，执行 `dotnet new acduimodule -n MyCompany.MyModule` 可脚手架生成模块。

## 快速开始（OWIN / net48）

```csharp
app.AddUiModules()
   .AddModule<MyUiModule>()
   .SetAuthorization("/MyModule", new LocalRequestsOnlyAuthorizationFilter());
app.UseUiModules();
```

## Docker

```bash
docker build -f docker/Dockerfile -t aspnetcoredashboard-sample .
docker run -p 8080:8080 aspnetcoredashboard-sample
# 打开 http://localhost:8080/Dashboard/
```

## 示例项目

| 项目 | 说明 |
|------|------|
| `AspNetCoreDashboardLibrarySamples` | `SampleUiModule` + `DiagnosticsUiModule`（netstandard2.0） |
| `AspNetCoreDashboardWebSamples` | ASP.NET Core 最小宿主（net8/9/10） |
| `AspNetCoreDashboardOwinSamples` | OWIN 自宿主，端口 1101 |

运行 Web 示例：

```bash
dotnet run --project Samples/AspNetCoreDashboardWebSamples
# 打开 http://localhost:5000/Dashboard/
```

详见 [Samples/README.md](Samples/README.md)。

## 文档

- [架构说明](docs/ARCHITECTURE.md)
- [模块作者指南](docs/MODULE_AUTHOR_GUIDE.md)
- [实用食谱](docs/COOKBOOK.md)
- [故障排查](docs/TROUBLESHOOTING.md)
- [1.0.0-beta-19 → beta-20 迁移指南](docs/MIGRATION.md)
- [安全指南](docs/SECURITY.md)
- [版本策略](docs/VERSIONING.md)
- [变更日志](CHANGELOG.md)

## 许可证

MIT — 详见 [LICENSE](LICENSE)。
