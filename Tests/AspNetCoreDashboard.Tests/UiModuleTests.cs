using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboardLibrarySamples;
using Xunit;

namespace AspNetCoreDashboard.Tests
{
  public class UiRouteTableTests
  {
    [Fact]
    public void TryMatch_returns_first_registered_route_in_order()
    {
      var table = new UiRouteTable();
      table.Add(UiHttpMethod.Get, "/api/list", _ => Task.CompletedTask);
      table.Add(UiHttpMethod.Get, "/.*", _ => Task.CompletedTask);

      Assert.True(table.TryMatch("GET", "/api/list", out _, out _));
    }

    [Fact]
    public void TryMatch_distinguishes_http_methods()
    {
      var table = new UiRouteTable();
      table.Add(UiHttpMethod.Post, "/api/list", _ => Task.CompletedTask);

      Assert.False(table.TryMatch("GET", "/api/list", out _, out _));
      Assert.True(table.TryMatch("POST", "/api/list", out _, out _));
    }

    [Fact]
    public void TryMatch_supports_put_and_delete()
    {
      var table = new UiRouteTable();
      table.Add(UiHttpMethod.Put, "/api/item", _ => Task.CompletedTask);
      table.Add(UiHttpMethod.Delete, "/api/item", _ => Task.CompletedTask);

      Assert.True(table.TryMatch("PUT", "/api/item", out _, out _));
      Assert.True(table.TryMatch("DELETE", "/api/item", out _, out _));
    }

    [Fact]
    public void TryMatch_supports_template_parameters()
    {
      var table = new UiRouteTable();
      table.Add(UiHttpMethod.Get, "/api/items/{id}", _ => Task.CompletedTask);

      Assert.True(table.TryMatch("GET", "/api/items/42", out var match, out _));
      Assert.Equal("42", match.Groups["id"].Value);
    }

    [Fact]
    public void TryMatch_head_falls_back_to_get_handler()
    {
      var table = new UiRouteTable();
      table.Add(UiHttpMethod.Get, "/api/ping", _ => Task.CompletedTask);
      Assert.True(table.TryMatch("HEAD", "/api/ping", out _, out _));
    }
  }

  public class UiRoutePatternTests
  {
    [Fact]
    public void ToRegexPattern_supports_legacy_wildcard_patterns()
    {
      var pattern = UiRoutePattern.ToRegexPattern("/.*");
      Assert.Equal("^/.*$", pattern);
    }

    [Fact]
    public void ToRegexPattern_supports_template_parameters()
    {
      var pattern = UiRoutePattern.ToRegexPattern("/api/items/{id}");
      Assert.Contains("(?<id>[^/]+)", pattern);
    }
    [Fact]
    public void ToRegexPattern_supports_int_constraint()
    {
      var pattern = UiRoutePattern.ToRegexPattern("/api/items/{id:int}");
      Assert.Matches(new Regex(pattern, RegexOptions.IgnoreCase), "/api/items/42");
      Assert.DoesNotMatch(new Regex(pattern, RegexOptions.IgnoreCase), "/api/items/abc");
    }
  }

  internal sealed class TestUiContext : IUiContext
  {
    public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();
    public IServiceProvider Services => null;
    public string Method => "GET";
    public string Path => "/";
    public Match RouteMatch { get; set; }
    public ClaimsPrincipal User { get; set; }
    public System.Threading.CancellationToken RequestAborted => System.Threading.CancellationToken.None;
    public string LocalIpAddress { get; set; }
    public string RemoteIpAddress { get; set; }
    public int StatusCode { get; set; }
    public Task<string> GetQueryAsync(string name) => Task.FromResult<string>(null);
    public Task<string> GetFormValueAsync(string name) => Task.FromResult<string>(null);
    public Task<IUiFormFile> GetFormFileAsync(string name) => Task.FromResult<IUiFormFile>(null);
    public Task<string> ReadBodyAsStringAsync() => Task.FromResult(string.Empty);
    public Task<System.IO.Stream> OpenRequestBodyAsync() => Task.FromResult<System.IO.Stream>(System.IO.Stream.Null);
    public Task<T> ReadJsonAsync<T>() => Task.FromResult(default(T));
    public string GetRequestHeader(string name) => RequestHeaders.TryGetValue(name, out var value) ? value : null;
    public string GetRequestCookie(string name) => null;
    public void SetResponseHeader(string name, string value) { }
    public void SetCookie(string name, string value, DateTimeOffset? expires = null, string path = null, bool httpOnly = false, bool secure = false, UiCookieSameSite sameSite = UiCookieSameSite.Unspecified) { }
    public Task WriteAsync(string content, string contentType = "text/plain") => Task.CompletedTask;
    public Task WriteJsonAsync(object value) => Task.CompletedTask;
    public Task WriteStreamAsync(System.IO.Stream stream, string contentType, string downloadFileName = null) => Task.CompletedTask;
    public Task RedirectAsync(string location, bool permanent = false) => Task.CompletedTask;
  }

  public class TrustedForwardedHeadersAuthorizationFilterTests
  {
    [Fact]
    public void Authorize_allows_client_when_proxy_is_trusted()
    {
      var filter = new TrustedForwardedHeadersAuthorizationFilter("10.0.0.1");
      var context = new TestUiContext
      {
        RemoteIpAddress = "10.0.0.1",
        RequestHeaders = new Dictionary<string, string> { ["X-Forwarded-For"] = "127.0.0.1" }
      };
      Assert.True(filter.Authorize(context));
    }
  }

  public class RequireRoleAuthorizationFilterTests
  {
    [Fact]
    public void Authorize_requires_role()
    {
      var filter = new RequireRoleAuthorizationFilter("Admin");
      var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "User") }, "Test");
      var context = new TestUiContext { User = new ClaimsPrincipal(identity) };
      Assert.False(filter.Authorize(context));
    }
  }

  public class EmbeddedResourceCacheTests
  {
    [Fact]
    public void GetBytes_returns_cached_content()
    {
      EmbeddedResourceCache.Clear();
      var assembly = typeof(SampleUiModule).Assembly;
      var resource = "AspNetCoreDashboardLibrarySamples.Content.index.html";
      var first = EmbeddedResourceCache.GetBytes(assembly, resource);
      var second = EmbeddedResourceCache.GetBytes(assembly, resource);
      Assert.NotNull(first);
      Assert.Same(first, second);
    }
  }

  public class LocalRequestsOnlyAuthorizationFilterTests
  {
    [Fact]
    public void Authorize_allows_localhost()
    {
      var filter = new LocalRequestsOnlyAuthorizationFilter();
      var context = new TestUiContext { RemoteIpAddress = "127.0.0.1", LocalIpAddress = "192.168.1.1" };
      Assert.True(filter.Authorize(context));
    }

    [Fact]
    public void Authorize_denies_unknown_remote_ip()
    {
      var filter = new LocalRequestsOnlyAuthorizationFilter();
      var context = new TestUiContext { RemoteIpAddress = null, LocalIpAddress = "192.168.1.1" };
      Assert.False(filter.Authorize(context));
    }
  }

  public class UiModuleMountTrackerTests
  {
    [Fact]
    public void Register_throws_when_path_prefix_duplicated()
    {
      var tracker = new UiModuleMountTracker();
      tracker.Register("/Dashboard");

      Assert.Throws<InvalidOperationException>(() => tracker.Register("/Dashboard/"));
    }
  }

  public class UiModuleAssemblyExtensionsTests
  {
    [Fact]
    public void GetUiModuleTypes_finds_sample_modules()
    {
      var types = typeof(SampleUiModule).Assembly.GetUiModuleTypes();
      Assert.Contains(typeof(SampleUiModule), types);
      Assert.Contains(typeof(DiagnosticsUiModule), types);
    }

    [Fact]
    public void AddModulesFromAssembly_registers_all_modules()
    {
      var registry = new UiModuleRegistry();
      registry.AddModulesFromAssembly(typeof(SampleUiModule).Assembly);
      Assert.Equal(2, registry.Modules.Count);
    }
  }
}
