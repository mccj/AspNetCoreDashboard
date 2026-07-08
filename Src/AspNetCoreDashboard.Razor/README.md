# AspNetCoreDashboard.Razor

**可选预览包**，为 UI 模块提供基于嵌入 HTML 模板的“类 Razor”页面路由。当前**不**包含完整 Razor 编译管线。

| 属性 | 值 |
|------|-----|
| NuGet | `AspNetCoreDashboard.Razor` |
| TFM | `net8.0`、`net9.0`、`net10.0` |
| 依赖 | `AspNetCoreDashboard`、`AspNetCoreDashboard.Abstractions` |
| 状态 | 预览 |

## 功能概览

- `MapRazorPage(pattern, pageType)`：注册 GET 路由
- 在模块程序集中查找名为 `{PageType.Name}.html` 的嵌入资源并作为 HTML 返回
- 找不到模板时返回占位 HTML 页面（提示缺少嵌入资源）

## 使用方法

### 1. 引用包

宿主与模块项目需已使用 `AspNetCoreDashboard` 宿主。

```xml
<PackageReference Include="AspNetCoreDashboard.Razor" Version="3.6.0" />
```

### 2. 嵌入页面模板

将 `Home.html` 设为嵌入资源，逻辑名例如：

```
MyModule.Content.Home.html
```

### 3. 注册路由

```csharp
using AspNetCoreDashboard.Razor;

public void Configure(IUiModuleRegistration builder)
{
    builder.MapEmbeddedUi(assembly, "MyModule.Content")
           .MapRazorPage("/home", typeof(HomePage)); // HomePage 为标记类型，无实例
}
```

`MapRazorPage` 通过 `pageType.Name` 匹配 `HomePage.html` 嵌入文件。

## 工作原理

```
GET /{PathPrefix}/home
  → 查找 *HomePage.html 嵌入资源
  → 命中：返回模板 HTML
  → 未命中：返回内置占位页
```

## 注意事项

- **非完整 Razor**：不支持 `.cshtml` 编译、`@model`、布局或 Tag Helper；仅为嵌入 HTML 模板服务。完整 Razor 编译 hosting 列为后续迭代。
- 仅适用于 **ASP.NET Core** 宿主；OWIN 请自行 `MapGet` 返回 HTML。
- 页面类型仅用作名称标记，无需实现接口或创建实例。
- 生产环境 SPA 场景通常使用 `MapEmbeddedUi` + `MapSpaFallback` 即可。
