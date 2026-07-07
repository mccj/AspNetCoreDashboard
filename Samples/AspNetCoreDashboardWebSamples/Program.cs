using AspNetCoreDashboardWebSamples;

var builder = WebApplication.CreateBuilder(args);
WebSampleHostConfiguration.ConfigureServices(builder);

var app = builder.Build();
WebSampleHostConfiguration.ConfigurePipeline(app);

app.Run();

public partial class Program { }
