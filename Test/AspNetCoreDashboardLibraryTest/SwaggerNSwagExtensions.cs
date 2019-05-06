using Microsoft.Extensions.DependencyInjection;
using AspNetCoreDashboard;
using Microsoft.AspNetCore.Builder;
using AspNetCoreDashboard.Dashboard;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DashboardExtensions
    {
        public static IApplicationBuilder UseDashboardTest(this IApplicationBuilder app)
        {
            var assembly = typeof(DashboardExtensions).GetTypeInfo().Assembly;
            var contentFolderNamespace = GetContentFolderNamespace();
            var routes = new AspNetCoreDashboard.Dashboard.RouteCollection();
            routes.Add("/", new EmbeddedResourceDispatcher(System.Net.Mime.MediaTypeNames.Text.Html, assembly, GetContentResourceName("index.html")));
            //app.UseStaticFiles(new StaticFileOptions { RequestPath = "/aa/bb", ServeUnknownFileTypes = true, DefaultContentType = "application/x-msdownload", FileProvider = new EmbeddedFileProvider(assembly, "AspNetCoreDashboardLibraryTest") });
            routes.AddEmbeddedResource("/(?<path>.+\\.css)", "text/css", assembly, contentFolderNamespace);
            routes.AddEmbeddedResource("/(?<path>.+\\.js)",  "application/javascript", assembly, contentFolderNamespace);
            routes.AddEmbeddedResource("/(?<path>.+\\.png)", "image/png", assembly, contentFolderNamespace);
            routes.AddEmbeddedResource("/(?<path>.+\\.gif)", System.Net.Mime.MediaTypeNames.Image.Gif, assembly, contentFolderNamespace);
            routes.AddEmbeddedResource("/(?<path>.+\\.jpg)", System.Net.Mime.MediaTypeNames.Image.Jpeg, assembly, contentFolderNamespace);
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

            //AspNetCore.StaticFiles.FileExtensionContentTypeProvider f; f.TryGetContentType()
            app.UseMapDashboard("/Dashboard", null, routes);
            return app;
        }
        private static string GetContentResourceName(string resourceName)
        {
            return GetContentResourceName("", resourceName);
        }
        private static string GetContentResourceName(string contentFolder, string resourceName)
        {
            return $"{GetContentFolderNamespace(contentFolder)}.{resourceName}";
        }
        private static string GetContentFolderNamespace(string contentFolder=null)
        {
            var assemblyName = //"TrafficFlowStatistics";
                typeof(DashboardExtensions).Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

            return $"{assemblyName}.Content{(string.IsNullOrWhiteSpace(contentFolder) ? "" : ".")}{contentFolder}";
        }


        //private static IApplicationBuilder UseNSwagReDoc(this IApplicationBuilder app, string path = null)
        //{
        //    // Config with support for multiple documents
        //    var swaggerDocumentRegistration = app.ApplicationServices.GetServices<SwaggerDocumentRegistration>();
        //    foreach (var item in swaggerDocumentRegistration)
        //    {
        //        app.UseReDoc(config =>
        //        {
        //            if (!string.IsNullOrWhiteSpace(path))
        //                config.Path = path;
        //            config.DocumentPath = config.DocumentPath.Replace("{documentName}", item.DocumentName);
        //            config.Path = config.Path + "/ReDoc";

        //            config.TransformToExternalPath = (internalUiRoute, request) =>
        //            {
        //                // The header X-External-Path is set in the nginx.conf file
        //                var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
        //                return externalPath + internalUiRoute;
        //            };
        //        })
        //        ;
        //    }


        //            // Config with single document
        //            //app.UseReDoc(config =>
        //            //{
        //            //    config.SwaggerRoute = "/swagger/v1/swagger.json";
        //            //    config.TransformToExternalPath = (internalUiRoute, request) =>
        //            //    {
        //            //        // The header X-External-Path is set in the nginx.conf file
        //            //        var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
        //            //        return externalPath + internalUiRoute;
        //            //    };
        //            //});


        //            return app;
        //        }

        //        private static IApplicationBuilder UseNSwagSwaggerUI3(this IApplicationBuilder app, string path = null)
        //        {
        //            // Config with support for multiple documents
        //            app.UseSwaggerUi3(config =>
        //            {
        //                if (!string.IsNullOrWhiteSpace(path))
        //                    config.Path = path;
        //                config.TransformToExternalPath = (internalUiRoute, request) =>
        //                {
        //                    // The header X-External-Path is set in the nginx.conf file
        //                    var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
        //                    return externalPath + internalUiRoute;
        //                };
        //            });

        //            return app;
        //        }

        //        private static IApplicationBuilder UseNSwagSwagger(IApplicationBuilder app, string path = null)
        //        {
        //            app.UseSwagger(config =>
        //            {
        //                if (!string.IsNullOrWhiteSpace(path))
        //                    config.Path = path;

        //                config.PostProcess = (document, request) =>
        //                {
        //                    if (request.Headers.ContainsKey("X-External-Host"))
        //                    {
        //                        // Change document server settings to public
        //                        document.Host = request.Headers["X-External-Host"].First();
        //                        document.BasePath = request.Headers["X-External-Path"].First();
        //                    }
        //                };
        //            });
        //            return app;
        //        }

        //        private static void _settings(SwaggerDocumentSettings document, string version = "v1", string state = "(稳定版)")
        //        {
        //            document.Title = "天使项目 API 文档" + state;
        //            document.Description = @"天使项目 API 文档,可以使用API Key来授权测试。

        //# Introduction
        //This API is documented in **OpenAPI format** and is based on

        //# Authentication

        // Petstore offers two forms of authentication:
        //      - API Key
        //      - OAuth2
        //    OAuth2 - an open protocol to allow secure authorization in a simple
        //    and standard method from web, mobile and desktop applications.

        //";
        //            document.Version = version;


        //            document.PostProcess = (f) =>
        //            {
        //                f.Info.TermsOfService = "http://www.weberp.com.cn";
        //                f.Info.Contact = new NSwag.SwaggerContact { Email = "mccj@weberp.com.cn", Name = "The KeWei Team", Url = "http://www.weberp.com.cn" };
        //                f.Info.License = new NSwag.SwaggerLicense { Name = "Apache 2.0", Url = "http://www.apache.org/licenses/LICENSE-2.0.html" };

        //                f.Info.ExtensionData = new Dictionary<string, object>();
        //                f.Info.ExtensionData.Add("x-logo", new { url = "https://rebilly.github.io/ReDoc/petstore-logo.png", altText = "Petstore logo" });
        //            };

        //#pragma warning disable CS0618 // 类型或成员已过时
        //            document.DefaultEnumHandling = NJsonSchema.EnumHandling.CamelCaseString;
        //            //document.DefaultPropertyNameHandling = NJsonSchema.PropertyNameHandling.CamelCase;
        //            ////document.DefaultReferenceTypeNullHandling = NJsonSchema.ReferenceTypeNullHandling.Null;
        //#pragma warning restore CS0618 // 类型或成员已过时
        //        }
        //    }

        //    public class ReDocCodeSampleAttribute : SwaggerOperationProcessorAttribute
        //    {
        //        public ReDocCodeSampleAttribute(string language, string source)
        //            : base(typeof(ReDocCodeSampleAppender), language, source)
        //        {
        //        }

        //        internal class ReDocCodeSampleAppender : IOperationProcessor
        //        {
        //            private readonly string _language;
        //            private readonly string _source;
        //            private const string ExtensionKey = "x-code-samples";

        //            public ReDocCodeSampleAppender(string language, string source)
        //            {
        //                //var document = NSwag.SwaggerDocument.FromJsonAsync("...").Result;
        //                //var settings = new NSwag.CodeGeneration.CSharp.SwaggerToCSharpClientGeneratorSettings
        //                //{
        //                //    ClassName = "MyClass",
        //                //    CSharpGeneratorSettings =
        //                //    {
        //                //        Namespace = "MyNamespace"
        //                //    }
        //                //};
        //                //var generator = new NSwag.CodeGeneration.CSharp.SwaggerToCSharpClientGenerator(document, settings);



        //                _language = language;
        //                _source = source;
        //            }

        //            public Task<bool> ProcessAsync(OperationProcessorContext context)
        //            {
        //                if (context.OperationDescription.Operation.ExtensionData == null)
        //                    context.OperationDescription.Operation.ExtensionData = new Dictionary<string, object>();

        //                var data = context.OperationDescription.Operation.ExtensionData;
        //                if (!data.ContainsKey(ExtensionKey))
        //                    data[ExtensionKey] = new List<ReDocCodeSample>();

        //                var samples = (List<ReDocCodeSample>)data[ExtensionKey];
        //                samples.Add(new ReDocCodeSample
        //                {
        //                    Language = _language,
        //                    Source = _source,
        //                });

        //                return Task.FromResult(true);
        //            }
        //        }

        //        internal class ReDocCodeSample
        //        {
        //            [JsonProperty("lang")]
        //            public string Language { get; set; }

        //            [JsonProperty("source")]
        //            public string Source { get; set; }
        //        }
    }
}
