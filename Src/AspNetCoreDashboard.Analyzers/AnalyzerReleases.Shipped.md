## Release 1.0

### 新增规则

规则 ID | 类别 | 严重性 | 说明
--------|------|--------|------
ACD001 | AspNetCoreDashboard | Error | PathPrefix 必须以 '/' 开头
ACD002 | AspNetCoreDashboard | Warning | 找不到回退 index 资源
ACD003 | AspNetCoreDashboard | Info | 嵌入式 UI 命名空间应以 '.Content' 结尾
ACD004 | AspNetCoreDashboard | Warning | UI 模块路由重复
ACD005 | AspNetCoreDashboard | Warning | SPA 回退需要先调用 MapEmbeddedUi
ACD006 | AspNetCoreDashboard | Warning | PathPrefix 与 UiModuleAttribute 不一致
ACD007 | AspNetCoreDashboard | Info | 带 UiModule 特性的模块应调用 MapEmbeddedUi
