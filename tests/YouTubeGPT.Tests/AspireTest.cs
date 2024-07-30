using YouTubeGPT.Tests.NUnitExtensions;

namespace YouTubeGPT.Tests;
public abstract class AspireTest
{
    protected DistributedApplication _app = null!;
    protected ResourceNotificationService _resourceNotificationService = null!;

    [SetUp]
    public async Task Setup()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.YouTubeGPT_AppHost>();
        appHost.Services
            .ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddStandardResilienceHandler())
            .AddLogging(builder => builder.AddNUnit());

        _app = await appHost.BuildAsync();
        _resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
