using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreDashboard.Abstractions;

namespace AspNetCoreDashboardLibrarySamples
{
  /// <summary>
  /// 演示嵌入式静态文件、SPA 回退、JSON API 与命令端点的示例 UI 模块。
  /// </summary>
  [UiModule("/Dashboard", ContentNamespace = "AspNetCoreDashboardLibrarySamples.Content")]
  public sealed class SampleUiModule : IUiModule
  {
    public string PathPrefix => "/Dashboard";

    public void Configure(IUiModuleRegistration builder)
    {
      var assembly = typeof(SampleUiModule).Assembly;
      var contentNamespace = GetContentFolderNamespace();

      builder.MapEmbeddedUi(assembly, contentNamespace)
             .MapSpaFallback($"{contentNamespace}.index.html")
             .ExcludeSpaFallbackExtensions(".json", ".map")
             .MapGet("/health", HandleHealth)
             .MapGet("/api/status", HandleStatus)
             .MapPost("/api/upload", HandleUpload)
             .MapGet("/api/jobs", HandleJobsList)
             .MapPost("/api/jobs", HandleJobsCreate)
             .MapDelete("/api/jobs/{id:int}", HandleJobsDelete)
             .MapGet("/api/jobs/export.csv", HandleJobsExport)
             .MapGet("/FlowStatistics", HandleFlowStatisticsGet)
             .MapPost("/FlowStatistics", HandleFlowStatisticsPost)
             .MapGet("/api/items/{id:int}", HandleItemGet)
             .MapPut("/api/items/{id}", HandleItemPut)
             .MapPatch("/api/items/{id}", HandleItemPatch)
             .MapOptions("/api/items/{id}", HandleItemOptions)
             .MapDelete("/api/items/{id}", HandleItemDelete)
             .MapGet("/api/export/sample.txt", HandleExportSample);
    }

    private static Task HandleStatus(IUiContext context)
    {
      return context.WriteJsonAsync(new
      {
        ok = true,
        module = "Dashboard",
        embeddedUi = true
      });
    }

    private static Task HandleJobsList(IUiContext context)
    {
      return context.WriteJsonAsync(SampleJobStore.List());
    }

    private static async Task HandleJobsCreate(IUiContext context)
    {
      var body = await context.ReadBodyAsStringAsync();
      var name = TryReadJsonProperty(body, "name") ?? "job";
      var job = SampleJobStore.Add(name);
      context.StatusCode = 201;
      await context.WriteJsonAsync(job);
    }

    private static Task HandleJobsDelete(IUiContext context)
    {
      if (!int.TryParse(context.GetRouteValue("id"), out var id) || !SampleJobStore.Remove(id))
      {
        context.StatusCode = 404;
        return context.WriteJsonAsync(new { error = "not found" });
      }

      context.StatusCode = 204;
      return Task.CompletedTask;
    }

    private static Task HandleJobsExport(IUiContext context)
    {
      var bytes = Encoding.UTF8.GetBytes(SampleJobStore.ExportCsv());
      var stream = new MemoryStream(bytes, writable: false);
      return context.WriteStreamAsync(stream, "text/csv", "jobs.csv");
    }

    private static async Task HandleFlowStatisticsGet(IUiContext context)
    {
      var filter = await context.GetQueryAsync("filter");
      var orderBy = await context.GetQueryAsync("orderBy");
      await context.WriteAsync($"GET filter={filter}, orderBy={orderBy}");
    }

    private static async Task HandleFlowStatisticsPost(IUiContext context)
    {
      var filter = await context.GetFormValueAsync("filter");
      var orderBy = await context.GetFormValueAsync("orderBy");
      await context.WriteAsync($"POST filter={filter}, orderBy={orderBy}");
    }

    private static Task HandleItemGet(IUiContext context)
    {
      var id = context.GetRouteValue("id");
      return context.WriteJsonAsync(new { id, name = "Item " + id, source = "SampleUiModule" });
    }

    private static async Task HandleItemPut(IUiContext context)
    {
      var body = await context.ReadBodyAsStringAsync();
      await context.WriteAsync($"PUT item={context.GetRouteValue("id")} body={body}");
    }

    private static async Task HandleItemPatch(IUiContext context)
    {
      var body = await context.ReadBodyAsStringAsync();
      await context.WriteAsync($"PATCH item={context.GetRouteValue("id")} body={body}");
    }

    private static Task HandleItemOptions(IUiContext context)
    {
      context.SetResponseHeader("Allow", "GET,PUT,PATCH,DELETE,OPTIONS");
      return context.WriteAsync(string.Empty);
    }

    private static Task HandleItemDelete(IUiContext context)
    {
      context.StatusCode = 204;
      context.SetResponseHeader("X-Deleted-Item", context.GetRouteValue("id"));
      return Task.CompletedTask;
    }

    private static Task HandleExportSample(IUiContext context)
    {
      var bytes = Encoding.UTF8.GetBytes("sample export");
      var stream = new MemoryStream(bytes, writable: false);
      return context.WriteStreamAsync(stream, "text/plain", "sample.txt");
    }

    private static Task HandleHealth(IUiContext context)
    {
      UiSecurityHeaders.ApplyBaseline(context);
      return context.WriteAsync("ok");
    }

    private static async Task HandleUpload(IUiContext context)
    {
      var file = await context.GetFormFileAsync("file");
      await context.WriteAsync(file == null ? "none" : file.FileName);
    }

    private static string TryReadJsonProperty(string json, string propertyName)
    {
      if (string.IsNullOrWhiteSpace(json))
      {
        return null;
      }

      var marker = "\"" + propertyName + "\"";
      var index = json.IndexOf(marker, System.StringComparison.Ordinal);
      if (index < 0)
      {
        return null;
      }

      var colon = json.IndexOf(':', index + marker.Length);
      if (colon < 0)
      {
        return null;
      }

      var start = json.IndexOf('"', colon + 1);
      if (start < 0)
      {
        return null;
      }

      var end = json.IndexOf('"', start + 1);
      if (end < 0)
      {
        return null;
      }

      return json.Substring(start + 1, end - start - 1);
    }

    private static string GetContentFolderNamespace()
    {
      var assemblyName = typeof(SampleUiModule).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
          ?? typeof(SampleUiModule).Assembly.GetName().Name;
      return assemblyName + ".Content";
    }
  }
}
