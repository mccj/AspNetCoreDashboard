using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Owin.Authorization;
using AspNetCoreDashboard.Owin.Extensions;
using AspNetCoreDashboardLibrarySamples;
using Microsoft.AspNetCore.Builder;
using Microsoft.Owin.Hosting;
using Owin;
using Xunit;

namespace AspNetCoreDashboard.Owin.Tests.Integration
{
  internal sealed class DenyAllAuthorizationFilter : IUiAuthorizationFilter
  {
    public bool Authorize(IUiContext context) => false;
  }

  internal sealed class DenyPolicyAuthorizationAdapter : IOwinAuthorizationAdapter
  {
    public bool Authorize(string policyName, IUiContext context) => false;
  }

  public sealed class OwinIntegrationTests : IDisposable
  {
    private readonly IDisposable _server;
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public OwinIntegrationTests()
    {
      var port = GetFreeTcpPort();
      _baseUrl = $"http://127.0.0.1:{port}/";
      _server = WebApp.Start(_baseUrl, app =>
      {
        app.AddUiModules()
                 .AddModule<SampleUiModule>();
        app.UseUiModules();
      });
      _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public void Dispose()
    {
      _client?.Dispose();
      _server?.Dispose();
    }

    [Fact]
    public async Task Dashboard_root_returns_index_html()
    {
      var response = await _client.GetAsync("Dashboard/", TestContext.Current.CancellationToken);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var body = await response.Content.ReadAsStringAsync();
      Assert.Contains("<html", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Dashboard_static_css_returns_correct_content_type()
    {
      var response = await _client.GetAsync("Dashboard/css/sample.css", TestContext.Current.CancellationToken);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Dashboard_template_route_returns_route_value()
    {
      var response = await _client.GetAsync("Dashboard/api/items/99", TestContext.Current.CancellationToken);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      var body = await response.Content.ReadAsStringAsync();
      Assert.Contains("\"id\":\"99\"", body);
    }

    [Fact]
    public async Task Dashboard_put_route_reads_body()
    {
      using (var content = new StringContent("{\"name\":\"x\"}", Encoding.UTF8, "application/json"))
      {
        var response = await _client.PutAsync("Dashboard/api/items/3", content, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("PUT item=3", body);
      }
    }

    [Fact]
    public async Task Dashboard_delete_route_sets_status()
    {
      var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "Dashboard/api/items/5"), TestContext.Current.CancellationToken);
      Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_returns_401_when_authorization_denied()
    {
      var port = GetFreeTcpPort();
      var baseUrl = $"http://127.0.0.1:{port}/";
      using (var server = WebApp.Start(baseUrl, app =>
      {
        app.AddUiModules()
                 .AddModule<SampleUiModule>()
                 .SetAuthorization("/Dashboard", new DenyAllAuthorizationFilter());
        app.UseUiModules();
      }))
      using (var client = new HttpClient { BaseAddress = new Uri(baseUrl) })
      {
        var response = await client.GetAsync("Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
      }
    }

    [Fact]
    public async Task Dashboard_returns_403_when_role_missing()
    {
      var port = GetFreeTcpPort();
      var baseUrl = $"http://127.0.0.1:{port}/";
      using (var server = WebApp.Start(baseUrl, app =>
      {
        app.Use((context, next) =>
              {
                var identity = new System.Security.Claims.ClaimsIdentity(
                          new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "user") },
                          "Test");
                context.Request.User = new System.Security.Claims.ClaimsPrincipal(identity);
                return next();
              });
        app.AddUiModules()
                 .AddModule<SampleUiModule>()
                 .SetAuthorization("/Dashboard", new RequireRoleAuthorizationFilter("Admin"));
        app.UseUiModules();
      }))
      using (var client = new HttpClient { BaseAddress = new Uri(baseUrl) })
      {
        var response = await client.GetAsync("Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
      }
    }

    [Fact]
    public async Task Dashboard_returns_403_when_policy_denied_via_adapter()
    {
      var port = GetFreeTcpPort();
      var baseUrl = $"http://127.0.0.1:{port}/";
      using (var server = WebApp.Start(baseUrl, app =>
      {
        app.UseOwinAuthorizationAdapter(new DenyPolicyAuthorizationAdapter());
        app.AddUiModules()
                 .AddModule<SampleUiModule>()
                 .SetAuthorizationPolicy("/Dashboard", "AdminOnly");
        app.UseUiModules();
      }))
      using (var client = new HttpClient { BaseAddress = new Uri(baseUrl) })
      {
        var response = await client.GetAsync("Dashboard/", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
      }
    }

    [Fact]
    public async Task Dashboard_head_route_returns_content_length_without_body()
    {
      var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "Dashboard/api/export/sample.txt"), TestContext.Current.CancellationToken);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.True(response.Content.Headers.ContentLength > 0);
      var body = await response.Content.ReadAsStringAsync();
      Assert.Equal(string.Empty, body);
    }

    [Fact]
    public async Task Dashboard_upload_route_reads_multipart_file_on_owin()
    {
      var port = GetFreeTcpPort();
      var baseUrl = $"http://127.0.0.1:{port}/";
      const string boundary = "---------------------------owintest";
      var payload =
          boundary + "\r\n" +
          "Content-Disposition: form-data; name=\"file\"; filename=\"owin-upload.txt\"\r\n" +
          "\r\n" +
          "data\r\n" +
          boundary + "--\r\n";

      using (var server = WebApp.Start(baseUrl, app =>
      {
        app.AddUiModules().AddModule<SampleUiModule>();
        app.UseUiModules();
      }))
      using (var client = new HttpClient { BaseAddress = new Uri(baseUrl) })
      using (var request = new HttpRequestMessage(HttpMethod.Post, "Dashboard/api/upload"))
      {
        request.Content = new StringContent(payload, Encoding.UTF8);
        request.Content.Headers.ContentType =
            System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                "multipart/form-data; boundary=" + boundary);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("owin-upload.txt", await response.Content.ReadAsStringAsync());
      }
    }

    private static int GetFreeTcpPort()
    {
      var listener = new TcpListener(IPAddress.Loopback, 0);
      listener.Start();
      try
      {
        return ((IPEndPoint)listener.LocalEndpoint).Port;
      }
      finally
      {
        listener.Stop();
      }
    }
  }
}
