using System;
using System.Linq;
using System.Security.Claims;

namespace AspNetCoreDashboard.Abstractions
{
  /// <summary>要求当前用户至少属于指定角色之一。</summary>
  public sealed class RequireRoleAuthorizationFilter : IUiAuthorizationFilter
  {
    private readonly string[] _roles;

    /// <summary>创建角色要求筛选器。</summary>
    public RequireRoleAuthorizationFilter(params string[] roles)
    {
      if (roles == null || roles.Length == 0)
        throw new ArgumentException("至少需要一个角色。", nameof(roles));

      _roles = roles;
    }

    /// <inheritdoc />
    public bool Authorize(IUiContext context)
    {
      if (context == null) throw new ArgumentNullException(nameof(context));

      var user = context.User;
      if (user?.Identity?.IsAuthenticated != true)
        return false;

      return _roles.Any(role => user.IsInRole(role));
    }
  }
}
