using AspNetCoreDashboard;
using AspNetCoreDashboard.Dashboard;
using System.Reflection;
#if NETSTANDARD
using IAppBuilder = Microsoft.AspNetCore.Builder.IApplicationBuilder;
#endif

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class DashboardExtensions
    {
#if NETSTANDARD
        public static IAppBuilder UseDashboardSamples(this IAppBuilder app)
        {
            var assembly = typeof(DashboardExtensions).GetTypeInfo().Assembly;
            var contentFolderNamespace = GetContentFolderNamespace();

            //AspNetCore.StaticFiles.FileExtensionContentTypeProvider f; f.TryGetContentType()
            app.UseMapDashboard("/Dashboard", routes =>
            {
                routes.Add("", new RedirectDispatcher((uriMatch) => uriMatch.Value + "/"));
                //routes.Add("/aaaa", new RedirectDispatcher((uriMatch) => uriMatch.Value + "/"));

                routes.Add("/", new EmbeddedResourceDispatcher(System.Net.Mime.MediaTypeNames.Text.Html, assembly, GetContentResourceName("index.html")));
                //routes.Add("/", new PhysicalFileDispatcher(System.Net.Mime.MediaTypeNames.Text.Html, "Content/index.html"));
                //routes.Add("/aaaa/", new EmbeddedResourceDispatcher(System.Net.Mime.MediaTypeNames.Text.Html, assembly, GetContentResourceName("index.html")));
                ////app.UseStaticFiles(new StaticFileOptions { RequestPath = "/aa/bb", ServeUnknownFileTypes = true, DefaultContentType = "application/x-msdownload", FileProvider = new EmbeddedFileProvider(assembly, "AspNetCoreDashboardLibraryTest") });
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.css)", "text/css", GetContentFolderNamespace());
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.js)", "application/javascript", GetContentFolderNamespace());
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.png)", "image/png", GetContentFolderNamespace());
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.gif)", System.Net.Mime.MediaTypeNames.Image.Gif, GetContentFolderNamespace());
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.jpg)", System.Net.Mime.MediaTypeNames.Image.Jpeg, GetContentFolderNamespace());
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.woff2)", "font/woff2", GetContentFolderNamespace());
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.woff)", "application/font-woff", GetContentFolderNamespace());
                //routes.AddEmbeddedResource(assembly, "/(?<path>.+\\.png)", "image/png", GetContentFolderNamespace());
                //routes.AddEmbeddedDefaultResource(assembly, GetContentFolderNamespace(),"");

                routes.AddCommand("/FlowStatistics", async context =>
                 {
                     try
                     {
                         var filter = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("filter") : context.Request.GetQuery("filter");
                         var orderBy = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("orderBy") : context.Request.GetQuery("orderBy");
                         //var start = (context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("start") : context.Request.GetQuery("start")).AsInt32();
                         //var length = (context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("length") : context.Request.GetQuery("length")).AsInt32();
                         //var draw = (context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("draw") : context.Request.GetQuery("draw"))?.AsInt32();

                         //var _accessInfoServices = app.ApplicationServices.GetService<AccessInfoServices>();

                         //var data = _accessInfoServices.GetAccessRecord().Where(filter).OrderBy(orderBy);

                         //var d = Newtonsoft.Json.JsonConvert.SerializeObject(new
                         //{
                         //    Data = data.Skip(start).Take(length).ToArray(),
                         //    Total = data.Count(),
                         //    Draw = draw,
                         //});
                         await context.Response.WriteAsync("aaa");
                     }
                     catch (System.Exception ex)
                     {
                         //var d = Newtonsoft.Json.JsonConvert.SerializeObject(new { IsSuccess = false, ErrorMsg = ex.DetailMessage() });
                         await context.Response.WriteAsync(ex.Message);
                     }
                     return true;
                 });
                
                routes.AddRazorPage("/tttt", x => new AspNetCoreDashboardLibrarySamples.Views.Dashboard.Home());

                routes.AddEmbeddedResource(assembly, "/(?<path>.*)", string.Empty, GetContentFolderNamespace());
                //routes.AddEmbeddedResource("/libs/(?<path>.+\\.json)", "application/json", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
                //routes.AddEmbeddedResource("/libs/(?<path>.+\\.js)", "application/javascript", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
                //routes.AddEmbeddedResource("/libs/(?<path>.+\\.css)", "text/css", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
                //routes.AddEmbeddedResource("/libs/(?<path>.+\\.png)", "image/png", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
                //routes.AddEmbeddedResource("/libs/(?<path>.+\\.gif)", "image/gif", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
                //routes.AddEmbeddedResource("/themes/(?<path>.+\\.css)", "text/css", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.themes"));
                //routes.AddEmbeddedResource("/themes/(?<path>.+\\.png)", "image/png", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.themes"));
                //routes.AddEmbeddedResource("/src/templates/(?<path>.+\\.html)", System.Net.Mime.MediaTypeNames.Text.Html, GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.src.templates"));
                //routes.AddEmbeddedResource("/languages/(?<path>.+\\.json)", "application/json", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.languages"));
                //routes.Add("/config/filemanager.init.js", new EmbeddedResourceDispatcher("application/javascript", GetExecutingAssembly(), GetContentResourceName("RichFilemanager.config", "filemanager.init.js")));
                //routes.Add("/config/filemanager.config.json", new EmbeddedResourceDispatcher("application/json", GetExecutingAssembly(), GetContentResourceName("RichFilemanager.config", "filemanager.config.json")));
                //routes.Add("/config/filemanager.config.default.json", new EmbeddedResourceDispatcher("application/json", GetExecutingAssembly(), GetContentResourceName("RichFilemanager.config", "filemanager.config.default.json")));
                //routes.AddCommand("", context=> { context.Response.r});

            }, null);
            return app;
        }
#endif

        private static string GetContentResourceName(string resourceName)
        {
            return GetContentResourceName("", resourceName);
        }
        private static string GetContentResourceName(string contentFolder, string resourceName)
        {
            return $"{GetContentFolderNamespace(contentFolder)}.{resourceName}";
        }
        private static string GetContentFolderNamespace(string contentFolder = null)
        {
            var assemblyName = //"TrafficFlowStatistics";
                typeof(DashboardExtensions).Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

            return $"{assemblyName}.Content{(string.IsNullOrWhiteSpace(contentFolder) ? "" : ".")}{contentFolder}";
        }

    }
}

