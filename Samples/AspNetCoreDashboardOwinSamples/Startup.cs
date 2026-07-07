using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Owin.Authorization;
using AspNetCoreDashboard.Owin.Extensions;
using AspNetCoreDashboardLibrarySamples;
using Microsoft.AspNetCore.Builder;
using Microsoft.Owin;
using Owin;

namespace AspNetCoreDashboardOwinSamples
{
  /// <summary>
  /// OWIN 宿主示例。与 Web 示例共享同一套 UI 模块程序集。
  /// </summary>
  /// <remarks>
  /// 与 ASP.NET Core 的差异：
  /// <list type="bullet">
  /// <item>使用 <c>AddModulesFromAssembly</c> 扫描模块，无 DI 注册。</item>
  /// <item>基于 Policy 的授权需 <c>IOwinAuthorizationAdapter</c>；示例中同时演示 Filter 与 Adapter。</item>
  /// <item>根路径重定向在 <see cref="UseUiModules"/> 之前通过 OWIN 中间件实现。</item>
  /// </list>
  /// </remarks>
  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      app.Use(async (context, next) =>
      {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path == "/" || path == string.Empty)
        {
          context.Response.Redirect("/Dashboard/");
          return;
        }

        await next();
      });

      app.AddUiModules()
         .AddModulesFromAssembly(typeof(SampleUiModule).Assembly)
         .SetAuthorization("/Dashboard", new LocalRequestsOnlyAuthorizationFilter())
         .SetAuthorization("/Diagnostics", new LocalRequestsOnlyAuthorizationFilter())
         .SetAuthorizationPolicy("/Diagnostics", "LocalOnly");

      app.UseOwinAuthorizationAdapter(new LocalOnlyPolicyAdapter());
      app.UseUiModules();
    }
  }

  internal sealed class LocalOnlyPolicyAdapter : IOwinAuthorizationAdapter
  {
    private readonly LocalRequestsOnlyAuthorizationFilter _localOnly = new LocalRequestsOnlyAuthorizationFilter();

    public bool Authorize(string policyName, IUiContext context)
    {
      if (!string.Equals(policyName, "LocalOnly", System.StringComparison.Ordinal))
        return false;

      return _localOnly.Authorize(context);
    }
  }
}
