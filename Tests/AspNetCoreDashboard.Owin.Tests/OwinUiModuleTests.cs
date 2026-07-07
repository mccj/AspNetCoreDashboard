using System;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using Xunit;

namespace AspNetCoreDashboard.Owin.Tests
{
  public class OwinUiRouteTableTests
  {
    [Fact]
    public void TryMatch_matches_post_routes()
    {
      var table = new UiRouteTable();
      table.Add(UiHttpMethod.Post, "/FlowStatistics", _ => Task.CompletedTask);

      Assert.True(table.TryMatch("POST", "/FlowStatistics", out _, out _));
    }

    [Fact]
    public void TryMatch_supports_template_parameters()
    {
      var table = new UiRouteTable();
      table.Add(UiHttpMethod.Get, "/api/items/{id}", _ => Task.CompletedTask);

      Assert.True(table.TryMatch("GET", "/api/items/5", out var match, out _));
      Assert.Equal("5", match.Groups["id"].Value);
    }
  }

  public class EmbeddedResourcePathHelperTests
  {
    [Fact]
    public void ToResourceName_maps_nested_paths()
    {
      var name = EmbeddedResourcePathHelper.ToResourceName("MyApp.Content", "css/site.css");
      Assert.Equal("MyApp.Content.css.site.css", name);
    }
  }

  public class UiModuleRegistryTests
  {
    [Fact]
    public void GetAuthorization_returns_filters_for_registered_path()
    {
      var filter = new LocalRequestsOnlyAuthorizationFilter();
      var registry = new UiModuleRegistry()
          .AddModule(new TestModule())
          .SetAuthorization("/Test", filter);

      var filters = registry.GetAuthorization("/Test");
      Assert.Single(filters);
      Assert.Same(filter, filters[0]);
    }

    private sealed class TestModule : IUiModule
    {
      public string PathPrefix => "/Test";
      public void Configure(IUiModuleRegistration builder) { }
    }
  }
}
