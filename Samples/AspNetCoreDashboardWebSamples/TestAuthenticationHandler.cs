using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreDashboardWebSamples
{
  internal sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
  {
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
      var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test-user") }, Scheme.Name);
      var principal = new ClaimsPrincipal(identity);
      var ticket = new AuthenticationTicket(principal, Scheme.Name);
      return Task.FromResult(AuthenticateResult.Success(ticket));
    }
  }
}
