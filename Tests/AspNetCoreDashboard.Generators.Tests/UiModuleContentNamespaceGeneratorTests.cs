using System.Linq;
using AspNetCoreDashboard.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace AspNetCoreDashboard.Generators.Tests
{
  public sealed class UiModuleContentNamespaceGeneratorTests
  {
    [Fact]
    public void UiModule_attribute_emits_generated_constants()
    {
      var source = @"
using AspNetCoreDashboard.Abstractions;

[UiModule(""/Custom"", ContentNamespace = ""MyModule.Content"")]
public sealed class CustomModule : IUiModule
{
    public string PathPrefix => ""/Custom"";
    public void Configure(IUiModuleRegistration builder) { }
}";

      var generated = RunGenerator(source);
      Assert.Contains("GeneratedPathPrefix", generated);
      Assert.Contains("\"/Custom\"", generated);
      Assert.Contains("GeneratedContentNamespace", generated);
      Assert.Contains("MyModule.Content", generated);
    }

    private static string RunGenerator(string source)
    {
      var syntaxTree = CSharpSyntaxTree.ParseText(source);
      var references = new MetadataReference[]
      {
                MetadataReference.CreateFromFile(typeof(IUiModule).Assembly.Location)
      };

      var compilation = CSharpCompilation.Create(
          "GeneratorTests",
          new[] { syntaxTree },
          references,
          new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

      var generator = new AspNetCoreDashboard.Generators.UiModuleContentNamespaceGenerator();
      var driver = CSharpGeneratorDriver.Create(generator);
      driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);
      return string.Join("\n", output.SyntaxTrees.Select(t => t.ToString()));
    }
  }
}
