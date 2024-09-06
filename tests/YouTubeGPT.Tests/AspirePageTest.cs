using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;
using YouTubeGPT.Tests.NUnitExtensions;

namespace YouTubeGPT.Tests;

public abstract class AspirePageTest : PageTest
{
    protected DistributedApplication app = null!;
    protected ResourceNotificationService resourceNotificationService = null!;

    public override BrowserNewContextOptions ContextOptions() => new()
    {
        Locale = "en-US",
        ColorScheme = ColorScheme.Light,
        IgnoreHTTPSErrors = true
    };

    [SetUp]
    public async Task Setup()
    {
        await Context.Tracing.StartAsync(new()
        {
            Title = $"{TestContext.CurrentContext.Test.ClassName}.{TestContext.CurrentContext.Test.Name}",
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

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
        TestContext currentContext = TestContext.CurrentContext;

        bool failed = currentContext.Result.Outcome switch
        {
            ResultState state when state == ResultState.Error || state == ResultState.Failure => true,
            _ => false
        };

        await Context.Tracing.StopAsync(new()
        {
            Path = failed ? Path.Combine(
                currentContext.WorkDirectory,
                "playwright-traces",
                $"{currentContext.Test.ClassName}.{currentContext.Test.Name}.zip"
            ) : null,
        });

        await app.StopAsync();
        await app.DisposeAsync();
    }
}
