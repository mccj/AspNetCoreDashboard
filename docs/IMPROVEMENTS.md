# 改进建议

## 已完成

### v2 – v3.6

- 三包 + Testing / Analyzers / Generators / Razor / SystemWeb 架构
- `[UiModule]` 特性、启动校验、健康检查（含模块明细）、ActivitySource 可观测性
- System.Web 完整 API 路由执行与流式静态资源
- 静态资源流式输出、Range / Accept-Ranges、Compiled 路由 Regex、按方法分组路由表
- ASP.NET Core `EmbeddedStaticFileMiddleware`；OWIN `OwinUiModulePipelineMiddleware`
- OWIN `IOwinAuthorizationAdapter` 策略授权桥接
- 按模块 CSP、上传大小限制（ASP.NET Core + OWIN）
- Analyzers ACD001–ACD007、ACD001/ACD006 CodeFix
- Public API 基线（RS0026）、CI format / net9 测试 / 覆盖率收集
- 集成测试：健康检查明细、上传限制、OWIN 策略适配器

## 待办

### Razor 完整编译 hosting

当前 `MapRazorPage` 仍为嵌入 HTML 模板；完整 Razor 编译为后续迭代。

## 实施路线

```
阶段 1–6（已完成） → v3.6 流式静态文件、OWIN 管道、路由优化、测试补强
阶段 7（后续）     → Razor 完整实现
```
