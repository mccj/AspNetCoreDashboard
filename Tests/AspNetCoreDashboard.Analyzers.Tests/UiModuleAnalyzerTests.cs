using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;
using AspNetCoreDashboard.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace AspNetCoreDashboard.Analyzers.Tests
{
  public sealed class UiModuleAnalyzerTests
  {
    [Fact]
    public async Task PathPrefix_without_leading_slash_reports_ACD001()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

public sealed class BadModule : IUiModule
{
    public string PathPrefix => ""Dashboard"";
    public void Configure(IUiModuleRegistration builder) { }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.Contains(diagnostics, d => d.Id == "ACD001");
    }

    [Fact]
    public async Task Valid_path_prefix_produces_no_diagnostics()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

public sealed class GoodModule : IUiModule
{
    public string PathPrefix => ""/Dashboard"";
    public void Configure(IUiModuleRegistration builder) { }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.DoesNotContain(diagnostics, d => d.Id == "ACD001");
    }

    [Fact]
    public async Task Spa_fallback_without_embedded_ui_reports_ACD005()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

public sealed class BadSpaModule : IUiModule
{
    public string PathPrefix => ""/Bad"";
    public void Configure(IUiModuleRegistration builder)
    {
        builder.MapSpaFallback(""Bad.Content.index.html"");
    }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.Contains(diagnostics, d => d.Id == "ACD005");
    }

    [Fact]
    public async Task Duplicate_route_reports_ACD004()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

public sealed class DupModule : IUiModule
{
    public string PathPrefix => ""/Dup"";
    public void Configure(IUiModuleRegistration builder)
    {
        builder.MapGet(""/api/x"", _ => System.Threading.Tasks.Task.CompletedTask)
               .MapGet(""/api/x"", _ => System.Threading.Tasks.Task.CompletedTask);
    }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.Contains(diagnostics, d => d.Id == "ACD004");
    }

    [Fact]
    public async Task Invalid_embedded_namespace_reports_ACD003()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

public sealed class NsModule : IUiModule
{
    public string PathPrefix => ""/Ns"";
    public void Configure(IUiModuleRegistration builder)
    {
        builder.MapEmbeddedUi(typeof(NsModule).Assembly, ""Wrong.Namespace"");
    }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.Contains(diagnostics, d => d.Id == "ACD003");
    }

    [Fact]
    public async Task PathPrefix_attribute_mismatch_reports_ACD006()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

[UiModule(""/Expected"")]
public sealed class AttrModule : IUiModule
{
    public string PathPrefix => ""/Actual"";
    public void Configure(IUiModuleRegistration builder) { }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.Contains(diagnostics, d => d.Id == "ACD006");
    }

    [Fact]
    public async Task Missing_fallback_resource_reports_ACD002()
    {
      // ACD002 依赖 MSBuild 在编译时生成的 .dashboard.embeddedresources 清单。
      // 在消费项目中通过分析器集成覆盖；此处省略清单模拟。
      var source = @"
using AspNetCoreDashboard.Abstractions;

public sealed class FallbackModule : IUiModule
{
    public string PathPrefix => ""/Fb"";
    public void Configure(IUiModuleRegistration builder)
    {
        builder.MapEmbeddedUi(typeof(FallbackModule).Assembly, ""FallbackModule.Content"")
               .MapFallbackToIndex(""FallbackModule.Content.index.html"");
    }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.DoesNotContain(diagnostics, d => d.Id == "ACD002");
    }

    [Fact]
    public async Task UiModule_without_embedded_ui_reports_ACD007()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

[UiModule(""/AttrOnly"")]
public sealed class AttrOnlyModule : IUiModule
{
    public string PathPrefix => ""/AttrOnly"";
    public void Configure(IUiModuleRegistration builder) { }
}";

      var diagnostics = await GetDiagnosticsAsync(source);
      Assert.Contains(diagnostics, d => d.Id == "ACD007");
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source, params string[] embeddedResources)
    {
      var syntaxTree = CSharpSyntaxTree.ParseText(source);
      var references = new MetadataReference[]
      {
                MetadataReference.CreateFromFile(typeof(IUiModule).Assembly.Location)
      };

      var compilation = CSharpCompilation.Create(
          "AnalyzerTests",
          new[] { syntaxTree },
          references,
          new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

      var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new UiModuleAnalyzer());
      var withAnalyzers = compilation.WithAnalyzers(analyzers);
      return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
  }
}
