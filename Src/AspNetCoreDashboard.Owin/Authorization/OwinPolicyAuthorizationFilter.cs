using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboard.Owin.Authorization
{
  internal sealed class OwinPolicyAuthorizationFilter : IUiAuthorizationFilter
  {
    private readonly string _policyName;
    private readonly IOwinAuthorizationAdapter _adapter;

    public OwinPolicyAuthorizationFilter(string policyName, IOwinAuthorizationAdapter adapter)
    {
      _policyName = policyName;
      _adapter = adapter;
    }

    public bool Authorize(IUiContext context) => _adapter.Authorize(_policyName, context);
  }
}
