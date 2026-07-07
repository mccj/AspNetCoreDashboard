using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreDashboard.Authorization
{
  internal sealed class PolicyUiAuthorizationFilter : IUiAuthorizationFilter
  {
    private readonly string _policyName;

    public PolicyUiAuthorizationFilter(string policyName)
    {
      _policyName = policyName;
    }

    public bool Authorize(IUiContext context)
    {
      if (!(context is Hosting.AspNetCoreUiContext aspNetContext))
        return false;

      var httpContext = aspNetContext.GetHttpContext();
      var authorizationService = httpContext.RequestServices.GetService<IAuthorizationService>();
      if (authorizationService == null)
        return false;

      var result = authorizationService
          .AuthorizeAsync(httpContext.User, null, _policyName)
          .GetAwaiter()
          .GetResult();

      return result.Succeeded;
    }
  }
}
