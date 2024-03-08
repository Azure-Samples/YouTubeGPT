using YoutubeExplode;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Ingestion.Operations;

var builder = Host.CreateDefaultBuilder();

builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();

    services.AddScoped<BuildVectorDatabaseOperationHandler>();

    services.AddScoped<YoutubeClient>();
});

await builder.Build().StartAsync();
