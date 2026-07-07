using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace AspNetCoreDashboard.Tests.Integration
{
  public sealed class WebSampleFactory : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("Testing");
    }
  }

  public sealed class UnauthorizedWebSampleFactory : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("Unauthorized");
    }
  }

  public sealed class ForbiddenWebSampleFactory : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("Forbidden");
    }
  }

  public sealed class PolicyForbiddenWebSampleFactory : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("PolicyForbidden");
    }
  }

  public sealed class AspNetCorePolicyForbiddenWebSampleFactory : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("AspNetCorePolicyForbidden");
    }
  }

  public sealed class HealthChecksWebSampleFactory : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("HealthChecks");
    }
  }

  public sealed class UploadLimitedWebSampleFactory : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment("UploadLimited");
    }
  }
}
