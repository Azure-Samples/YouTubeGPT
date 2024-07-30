using YouTubeGPT.Shared;
using YouTubeGPT.Tests.NUnitExtensions;

namespace YouTubeGPT.Tests;

public class AppHostHealthCheckTests
{
    [Test]
    public async Task ClientAppStartsAndRespondsWithOk()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.YouTubeGPT_AppHost>();
        appHost.Services
            .ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddStandardResilienceHandler())
            .AddLogging(builder => builder.AddNUnit());

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        
        var httpClient = app.CreateHttpClient(ServiceNames.YouTubeGPTClient);
        await resourceNotificationService.WaitForResourceAsync(ServiceNames.YouTubeGPTClient, KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        var response = await httpClient.GetAsync("/");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task IngestionAppStartsAndRespondsWithOk()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.YouTubeGPT_AppHost>();
        appHost.Services
            .ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddStandardResilienceHandler())
            .AddLogging(builder => builder.AddNUnit());

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(ServiceNames.YouTubeGPTIngestion);
        await resourceNotificationService.WaitForResourceAsync(ServiceNames.YouTubeGPTIngestion, KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        var response = await httpClient.GetAsync("/");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
