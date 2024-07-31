using Microsoft.Playwright.NUnit;
using YouTubeGPT.Tests.NUnitExtensions;

namespace YouTubeGPT.Tests;

public abstract class AspirePageTest : PageTest
{
    protected DistributedApplication app = null!;
    protected ResourceNotificationService resourceNotificationService = null!;

    [SetUp]
    public async Task Setup()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.YouTubeGPT_AppHost>();
        appHost.Services
            .ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddStandardResilienceHandler())
            .AddLogging(builder => builder.AddNUnit());

        app = await appHost.BuildAsync();
        resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }
}
