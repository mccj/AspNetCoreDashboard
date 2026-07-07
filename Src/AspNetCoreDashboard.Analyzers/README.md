# AspNetCoreDashboard.Analyzers

UI 模块作者的 **Roslyn 编译期分析器**，在编写 `IUiModule` 时尽早发现配置错误，并提供部分 CodeFix 自动修复。

| 属性 | 值 |
|------|-----|
| NuGet | `AspNetCoreDashboard.Analyzers` |
| TFM | `netstandard2.0` |
| 类型 | 开发依赖（`DevelopmentDependency`） |

## 功能概览

| 规则 ID | 严重性 | 说明 |
|---------|--------|------|
| **ACD001** | Error | `PathPrefix` 必须以 `/` 开头 |
| **ACD002** | Warning | `MapFallbackToIndex` 指定的嵌入资源不存在 |
| **ACD003** | Info | `MapEmbeddedUi` 命名空间建议以 `.Content` 结尾 |
| **ACD004** | Warning | 同一 HTTP 方法的路由重复注册 |
| **ACD005** | Warning | SPA 回退前未调用 `MapEmbeddedUi` |
| **ACD006** | Warning | `PathPrefix` 属性与 `[UiModule]` 不一致 |
| **ACD007** | Info | 带 `[UiModule]` 但未调用 `MapEmbeddedUi` |

**CodeFix**：ACD001（补全前导 `/`）、ACD006（与特性对齐）。

## 使用方法

在 UI 模块或宿主项目的 `.csproj` 中添加：

```xml
<PackageReference Include="AspNetCoreDashboard.Analyzers" Version="3.6.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

### ACD002 依赖嵌入资源清单

ACD002 需要 MSBuild 生成 `.dashboard.embeddedresources` 清单。请导入 Abstractions 包附带的目标：

```xml
<Import Project="$(PkgAspNetCoreDashboard_Abstractions)/build/AspNetCoreDashboard.EmbeddedContent.targets"
        Condition="'$(PkgAspNetCoreDashboard_Abstractions)' != ''" />
```

## 示例

以下代码触发 **ACD001**：

```csharp
public string PathPrefix => "Dashboard"; // 缺少前导 /
```

以下代码触发 **ACD005**：

```csharp
public void Configure(IUiModuleRegistration builder)
{
    builder.MapSpaFallback("MyModule.Content.index.html"); // 未先 MapEmbeddedUi
}
```

## 注意事项

- 分析器仅检查**语法可解析**的 `Configure` 方法；动态注册路由无法静态分析。
- ACD002 在未导入 `EmbeddedContent.targets` 时可能无法验证资源存在性。
- 规则 ID 与发布说明见 `AnalyzerReleases.Shipped.md`。
- 可选包：不引用不影响运行时，仅失去编译期提示。

## 相关文档

- [故障排查](../../docs/TROUBLESHOOTING.md)（分析器相关章节）
- [模块作者指南](../../docs/MODULE_AUTHOR_GUIDE.md)
