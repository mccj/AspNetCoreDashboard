# 版本与发布策略

## 包版本

以下包共享同一版本号，并一同发布：

| 包 | TFM |
|----|-----|
| `AspNetCoreDashboard.Abstractions` | netstandard2.0 |
| `AspNetCoreDashboard` | net8.0 |
| `AspNetCoreDashboard.Owin` | net48 |
| `AspNetCoreDashboard.Testing` | net8.0（可选） |
| `AspNetCoreDashboard.Analyzers` | netstandard2.0（可选） |
| `AspNetCoreDashboard.Razor` | net8.0（可选，预览） |

版本号定义于 [Directory.Build.props](../Directory.Build.props)。

## 3.0 破坏性变更

- **移除** 过时的 `UseMapDashboard` API 及全部 Hangfire 衍生的 `Compatibility/` 代码
- **新增** 路由模板语法 `/api/items/{id}` 与 Abstractions 中的共享 `UiRouteTable`
- **新增** `UseUiModule` 上的 `UiModuleMountTracker` 重复挂载检测

## 发布检查清单

1. 更新 [CHANGELOG.md](../CHANGELOG.md)
2. 在 `Directory.Build.props` 中提升 `<Version>`
3. 运行 CI（构建、测试、打包）
4. 推送标签 `vX.Y.Z` 以触发 [发布工作流](../.github/workflows/publish.yml)
5. 配置仓库密钥 `NUGET_API_KEY` 用于 NuGet.org 发布
