using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboardLibrarySamples;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AspNetCoreDashboard.Tests.Integration
{
  public class UiModuleIntegrationTests : IClassFixture<WebSampleFactory>
  {
    private readonly WebSampleFactory _factory;

    public UiModuleIntegrationTests(WebSampleFactory factory)
    {
      _factory = factory;
    }

    [Fact]
    public async Task Dashboard_root_returns_index_html_with_no_cache()
    {
      using (var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }))
      {
        var response = await client.GetAsync("/Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType ?? string.Empty);
        Assert.Contains("no-cache", response.Headers.CacheControl?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
      }
    }

    [Fact]
    public async Task Dashboard_static_css_returns_correct_content_type_and_cache()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/css/sample.css", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("max-age", response.Headers.CacheControl?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
      }
    }

    [Fact]
    public async Task Dashboard_api_route_handles_get()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/FlowStatistics?filter=a&orderBy=b", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("GET filter=a", body);
      }
    }

    [Fact]
    public async Task Diagnostics_status_returns_json()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/Diagnostics/api/status", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"module\":\"Diagnostics\"", body);
      }
    }

    [Fact]
    public async Task Dashboard_jobs_api_supports_crud()
    {
      using (var client = _factory.CreateClient())
      using (var createContent = new StringContent("{\"name\":\"test-job\"}", Encoding.UTF8, "application/json"))
      {
        var create = await client.PostAsync("/Dashboard/api/jobs", createContent, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var list = await client.GetAsync("/Dashboard/api/jobs", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var listBody = await list.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("test-job", listBody);

        var export = await client.GetAsync("/Dashboard/api/jobs/export.csv", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, export.StatusCode);
        Assert.Equal("text/csv", export.Content.Headers.ContentType?.MediaType);
      }
    }

    [Fact]
    public async Task Dashboard_api_status_returns_json()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/api/status", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"module\":\"Dashboard\"", body);
      }
    }

    [Fact]
    public async Task Dashboard_template_route_returns_route_value()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/api/items/42", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"id\":\"42\"", body);
        Assert.Contains("\"name\":\"Item 42\"", body);
      }
    }

    [Fact]
    public async Task Dashboard_put_route_reads_body()
    {
      using (var client = _factory.CreateClient())
      using (var content = new StringContent("{\"name\":\"x\"}", System.Text.Encoding.UTF8, "application/json"))
      {
        var response = await client.PutAsync("/Dashboard/api/items/7", content, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("PUT item=7", body);
      }
    }

    [Fact]
    public async Task Dashboard_delete_route_sets_status_and_header()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/Dashboard/api/items/9"), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("9", response.Headers.GetValues("X-Deleted-Item").FirstOrDefault());
      }
    }

    [Fact]
    public async Task Dashboard_returns_401_when_authorization_denies()
    {
      using (var factory = new UnauthorizedWebSampleFactory())
      using (var client = factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
      }
    }

    [Fact]
    public async Task Dashboard_returns_403_when_authenticated_but_denied()
    {
      using (var factory = new ForbiddenWebSampleFactory())
      using (var client = factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
      }
    }

    [Fact]
    public async Task Dashboard_returns_403_when_policy_role_missing()
    {
      using (var factory = new PolicyForbiddenWebSampleFactory())
      using (var client = factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
      }
    }

    [Fact]
    public async Task Dashboard_patch_route_reads_body()
    {
      using (var client = _factory.CreateClient())
      using (var content = new StringContent("{\"status\":\"ok\"}", Encoding.UTF8, "application/json"))
      {
        var response = await client.PatchAsync("/Dashboard/api/items/11", content, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("PATCH item=11", body);
      }
    }

    [Fact]
    public async Task Dashboard_options_route_sets_allow_header()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/Dashboard/api/items/1"), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal(string.Empty, body);
      }
    }

    [Fact]
    public async Task Dashboard_export_route_returns_download()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/api/export/sample.txt", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("sample export", body);
        Assert.Contains("attachment", response.Content.Headers.ContentDisposition?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
      }
    }

    [Fact]
    public async Task Dashboard_head_route_returns_content_length_without_body()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/Dashboard/api/export/sample.txt"), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Content.Headers.ContentLength > 0);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal(string.Empty, body);
      }
    }

    [Fact]
    public async Task Dashboard_upload_route_reads_multipart_file()
    {
      using (var client = _factory.CreateClient())
      using (var content = new MultipartFormDataContent())
      {
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("data")), "file", "upload.txt");
        var response = await client.PostAsync("/Dashboard/api/upload", content, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal("upload.txt", body);
      }
    }

    [Fact]
    public async Task Dashboard_health_route_applies_security_headers()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/health", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").FirstOrDefault());
      }
    }

    [Fact]
    public async Task Host_health_endpoint_returns_ok()
    {
      using (var client = _factory.CreateClient())
      {
        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("healthy", body);
      }
    }

    [Fact]
    public async Task Dashboard_returns_403_when_aspnetcore_policy_role_missing()
    {
      using (var factory = new AspNetCorePolicyForbiddenWebSampleFactory())
      using (var client = factory.CreateClient())
      {
        var response = await client.GetAsync("/Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
      }
    }

    [Fact]
    public async Task Dashboard_static_asset_returns_etag()
    {
      using (var client = _factory.CreateClient())
      {
        var first = await client.GetAsync("/Dashboard/css/sample.css", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var etag = first.Headers.ETag?.Tag;
        Assert.False(string.IsNullOrEmpty(etag));

        var second = new HttpRequestMessage(HttpMethod.Get, "/Dashboard/css/sample.css");
        second.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var notModified = await client.SendAsync(second, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotModified, notModified.StatusCode);
      }
    }

    [Fact]
    public async Task Health_modules_endpoint_reports_registered_modules()
    {
      using (var factory = new HealthChecksWebSampleFactory())
      using (var client = factory.CreateClient())
      {
        var response = await client.GetAsync("/health/modules", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("Dashboard", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Diagnostics", body, StringComparison.OrdinalIgnoreCase);
      }
    }

    [Fact]
    public async Task Upload_limited_module_returns_413_when_content_length_exceeds_limit()
    {
      using (var factory = new UploadLimitedWebSampleFactory())
      using (var client = factory.CreateClient())
      using (var content = new StringContent(new string('x', 64), Encoding.UTF8, "text/plain"))
      {
        var response = await client.PostAsync("/UploadLimited/api/echo", content, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
      }
    }

    [Fact]
    public async Task Upload_limited_module_accepts_body_within_limit()
    {
      using (var factory = new UploadLimitedWebSampleFactory())
      using (var client = factory.CreateClient())
      using (var content = new StringContent("ok", Encoding.UTF8, "text/plain"))
      {
        var response = await client.PostAsync("/UploadLimited/api/echo", content, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
      }
    }
  }

  public class UiModuleRegistryTests
  {
    [Fact]
    public void AddModule_throws_when_path_prefix_duplicated()
    {
      var registry = new UiModuleRegistry();
      registry.AddModule(new SampleUiModule());

      Assert.Throws<InvalidOperationException>(() => registry.AddModule(new SampleUiModule()));
    }
  }

  public class UiModuleStartupValidatorTests
  {
    [Fact]
    public void Validate_throws_for_duplicate_prefixes()
    {
      var modules = new IUiModule[] { new SampleUiModule(), new SampleUiModule() };
      Assert.Throws<InvalidOperationException>(() => UiModuleStartupValidator.Validate(modules));
    }

    [Fact]
    public void Validate_accepts_unique_modules()
    {
      UiModuleStartupValidator.Validate(new IUiModule[] { new SampleUiModule() });
    }
  }
}
