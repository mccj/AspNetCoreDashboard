using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Extensions;
using AspNetCoreDashboardLibrarySamples;
using AspNetCoreDashboardWebSamples;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspNetCoreDashboardWebSamples;

/// <summary>
/// Web 示例宿主配置。集成测试通过 <c>ASPNETCORE_ENVIRONMENT</c> 切换授权场景。
/// </summary>
public static class WebSampleHostConfiguration
{
  public static void ConfigureServices(WebApplicationBuilder builder)
  {
    builder.Services.AddUiModuleHosting(options =>
    {
      options.ApplySecurityHeaders = builder.Environment.IsProduction();
      options.EnableRequestLogging = builder.Environment.IsDevelopment();
    });

    if (!builder.Environment.IsEnvironment("Testing"))
    {
      builder.Services.AddHealthChecks().AddUiModulesHealthCheck();
    }

    var registry = builder.Services.AddUiModules()
        .AddModulesFromAssembly(builder.Services, typeof(SampleUiModule).Assembly);

    if (builder.Environment.IsEnvironment("UploadLimited"))
    {
      builder.Services.AddSingleton<IUiModule, UploadLimitedTestModule>();
      registry.AddModule(new UploadLimitedTestModule());
    }

    ConfigureAuthorization(builder, registry);
  }

  public static void ConfigurePipeline(WebApplication app)
  {
    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }

    if (RequiresAuthentication(app.Environment))
    {
      app.UseAuthentication();
    }

    if (app.Environment.IsEnvironment("AspNetCorePolicyForbidden"))
    {
      app.UseAuthorization();
    }

    app.UseUiModules();

    if (!app.Environment.IsEnvironment("Testing"))
    {
      app.MapUiModulesHealthChecks();
    }

    app.MapGet("/health", () => Results.Text("healthy", "text/plain"));
    app.MapGet("/", () => Results.Redirect("/Dashboard/"));
  }

  private static void ConfigureAuthorization(WebApplicationBuilder builder, UiModuleRegistry registry)
  {
    var environment = builder.Environment;

    if (environment.IsEnvironment("Unauthorized"))
    {
      registry.SetAuthorization("/Dashboard", new DenyAllAuthorizationFilter());
      registry.SetAuthorization("/Diagnostics", new DenyAllAuthorizationFilter());
      return;
    }

    if (environment.IsEnvironment("Forbidden"))
    {
      builder.Services.AddAuthentication("Test")
          .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", null);
      registry.SetAuthorization("/Dashboard", new DenyAllAuthorizationFilter());
      registry.SetAuthorization("/Diagnostics", new DenyAllAuthorizationFilter());
      return;
    }

    if (environment.IsEnvironment("PolicyForbidden"))
    {
      builder.Services.AddAuthentication("Test")
          .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", null);
      registry.SetAuthorization("/Dashboard", new RequireRoleAuthorizationFilter("Admin"));
      registry.SetAuthorization("/Diagnostics", new RequireRoleAuthorizationFilter("Admin"));
      return;
    }

    if (environment.IsEnvironment("AspNetCorePolicyForbidden"))
    {
      builder.Services.AddAuthentication("Test")
          .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", null);
      builder.Services.AddAuthorization(options =>
      {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
      });
      registry.SetAuthorizationPolicy("/Dashboard", "AdminOnly");
      registry.SetAuthorizationPolicy("/Diagnostics", "AdminOnly");
      return;
    }

    if (environment.IsEnvironment("Testing")
        || environment.IsEnvironment("HealthChecks")
        || environment.IsEnvironment("UploadLimited"))
    {
      return;
    }

    registry.SetAuthorization("/Dashboard", new LocalRequestsOnlyAuthorizationFilter());
    registry.SetAuthorization("/Diagnostics", new LocalRequestsOnlyAuthorizationFilter());
  }

  private static bool RequiresAuthentication(IHostEnvironment environment)
  {
    return environment.IsEnvironment("Forbidden")
        || environment.IsEnvironment("PolicyForbidden")
        || environment.IsEnvironment("AspNetCorePolicyForbidden");
  }
}
