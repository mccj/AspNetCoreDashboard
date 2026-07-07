using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>
  /// UI 模块及按路径划分的授权筛选器注册表，供宿主应用程序使用。
  /// </summary>
  public sealed class UiModuleRegistry
  {
    private readonly List<IUiModule> _modules = new List<IUiModule>();
    private readonly Dictionary<string, IUiAuthorizationFilter[]> _authorizationByPath =
        new Dictionary<string, IUiAuthorizationFilter[]>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _authorizationPolicyByPath =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 按插入顺序排列的已注册 UI 模块。
    /// </summary>
    public IReadOnlyList<IUiModule> Modules => _modules;

    /// <summary>
    /// 将模块实例添加到注册表。
    /// </summary>
    public UiModuleRegistry AddModule(IUiModule module)
    {
      if (module == null) throw new ArgumentNullException(nameof(module));
      ValidatePathPrefix(module.PathPrefix);
      EnsurePathPrefixAvailable(module.PathPrefix);
      _modules.Add(module);
      return this;
    }

    /// <summary>
    /// 将模块类型添加到注册表。
    /// </summary>
    public UiModuleRegistry AddModule<TModule>() where TModule : IUiModule, new()
    {
      return AddModule(new TModule());
    }

    /// <summary>
    /// 从程序集中添加所有具体 <see cref="IUiModule"/> 类型。
    /// </summary>
    public UiModuleRegistry AddModulesFromAssembly(Assembly assembly)
    {
      if (assembly == null) throw new ArgumentNullException(nameof(assembly));

      foreach (var moduleType in assembly.GetUiModuleTypes())
        AddModule((IUiModule)Activator.CreateInstance(moduleType));

      return this;
    }

    /// <summary>
    /// 为模块路径前缀设置授权筛选器。
    /// </summary>
    public UiModuleRegistry SetAuthorization(string pathPrefix, params IUiAuthorizationFilter[] filters)
    {
      _authorizationByPath[pathPrefix.TrimEnd('/')] = filters ?? Array.Empty<IUiAuthorizationFilter>();
      return this;
    }

    /// <summary>为模块路径前缀设置 ASP.NET Core 授权策略名称。</summary>
    public UiModuleRegistry SetAuthorizationPolicy(string pathPrefix, string policyName)
    {
      if (string.IsNullOrWhiteSpace(policyName))
        throw new ArgumentException("必须提供策略名称。", nameof(policyName));

      _authorizationPolicyByPath[pathPrefix.TrimEnd('/')] = policyName;
      return this;
    }

    /// <summary>
    /// 获取为模块路径前缀配置的授权筛选器。
    /// </summary>
    public IUiAuthorizationFilter[] GetAuthorization(string pathPrefix)
    {
      var key = pathPrefix.TrimEnd('/');
      if (_authorizationByPath.TryGetValue(key, out var filters))
        return filters;
      return Array.Empty<IUiAuthorizationFilter>();
    }

    /// <summary>获取为模块路径前缀配置的授权策略。</summary>
    public string? GetAuthorizationPolicy(string pathPrefix)
    {
      var key = pathPrefix.TrimEnd('/');
      return _authorizationPolicyByPath.TryGetValue(key, out var policy) ? policy : null;
    }

    private void EnsurePathPrefixAvailable(string pathPrefix)
    {
      var normalized = NormalizePathPrefix(pathPrefix);
      foreach (var existing in _modules)
      {
        if (string.Equals(NormalizePathPrefix(existing.PathPrefix), normalized, StringComparison.OrdinalIgnoreCase))
        {
          throw new InvalidOperationException(
              $"PathPrefix 为 '{existing.PathPrefix}' 的 UI 模块已注册。");
        }
      }
    }

    private static string NormalizePathPrefix(string pathPrefix)
    {
      return pathPrefix.TrimEnd('/');
    }

    private static void ValidatePathPrefix(string pathPrefix)
    {
      if (string.IsNullOrWhiteSpace(pathPrefix))
        throw new ArgumentException("必须提供 PathPrefix。", nameof(pathPrefix));
      if (!pathPrefix.StartsWith("/", StringComparison.Ordinal))
        throw new ArgumentException("PathPrefix 必须以 '/' 开头。", nameof(pathPrefix));
    }
  }
}
