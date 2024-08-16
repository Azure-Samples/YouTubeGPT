using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Npgsql;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Ingestion.Models;
using YouTubeGPT.Shared;

namespace YouTubeGPT.Tests;
public class IngestionTests : AspirePageTest
{
    // The .NET at Build 2024 playlist URL
    private const string YouTubeUrl = "https://www.youtube.com/playlist?list=PLdo4fOcmZ0oUZz7p8H1HsQjgv5tRRIvAS";

    [Test]
    public async Task CanIngestData()
    {
        await IngestYouTubeChannelData();
    }

    [Test]
    public async Task MetadataWillContainSingleRecord()
    {
        await IngestYouTubeChannelData();

        string? metadataConnectionString = await app.GetConnectionStringAsync(ServiceNames.MetadataDB);

        Assert.That(metadataConnectionString, Is.Not.Null);

        var optionsBuilder = new DbContextOptionsBuilder<MetadataDbContext>();
        optionsBuilder.UseNpgsql(metadataConnectionString);
        MetadataDbContext dbContext = new(optionsBuilder.Options);
        var metadata = await dbContext.Metadata.Select(m => m).ToListAsync();
        Assert.That(metadata, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ShouldHaveDefaultCountOfIngestedRecordsInSKMemory()
    {
        await IngestYouTubeChannelData();

        string? metadataConnectionString = await app.GetConnectionStringAsync(ServiceNames.MetadataDB);

        Assert.That(metadataConnectionString, Is.Not.Null);

        var optionsBuilder = new DbContextOptionsBuilder<MetadataDbContext>();
        optionsBuilder.UseNpgsql(metadataConnectionString);
        MetadataDbContext dbContext = new(optionsBuilder.Options);
        List<MemoryMetadata> metadata = await dbContext.Metadata.Select(m => m).ToListAsync();
        MemoryMetadata metadataRecord = metadata.First();

        string? vectorConnectionString = await app.GetConnectionStringAsync(ServiceNames.VectorDB);
        Assert.That(vectorConnectionString, Is.Not.Null);

        using var connection = new NpgsqlConnection(vectorConnectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"""SELECT COUNT(*) FROM public."{metadataRecord.CollectionName}_{Constants.DescriptionsCollectionSuffix}";""";
        long? count = (long?)await command.ExecuteScalarAsync();
        Assert.That(count, Is.EqualTo(10));
    }

    private async Task IngestYouTubeChannelData()
    {
        await resourceNotificationService.WaitForResourceAsync(ServiceNames.YouTubeGPTIngestion, KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

        var httpClient = app.CreateHttpClient(ServiceNames.YouTubeGPTIngestion);
        await Page.GotoAsync(httpClient.BaseAddress!.ToString());

        // Ensuring that Blazor properly loads, sometimes there seems to be a "flash" after it loads that resets controls
        //await Task.Delay(TimeSpan.FromSeconds(2));

        // Enter the data into the page and submit form
        await Page.GetByLabel("Channel URL").ClickAsync();
        await Page.GetByLabel("Channel URL").FillAsync(YouTubeUrl);
        await Page.GetByLabel("Channel URL").BlurAsync();

        var value = await Page.GetByLabel("Channel URL").InputValueAsync();

        if (value != YouTubeUrl)
        {
            // sometimes it clears the field once entered, so we'll re-enter it

            await Page.GetByLabel("Channel URL").ClickAsync();
            await Page.GetByLabel("Channel URL").FillAsync(YouTubeUrl);
            await Page.GetByLabel("Channel URL").BlurAsync();
        }

        await Page.GetByRole(AriaRole.Button, new() { Name = "Build Index" }).ClickAsync();

        // Text input should be disabled
        await Expect(Page.GetByRole(AriaRole.Textbox).First).ToBeDisabledAsync();

        // Wait for the progress bar to appear
        await Page.WaitForSelectorAsync("[aria-live='polite']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await Expect(Page.GetByTestId("process")).ToBeVisibleAsync();

        // Wait for the alert to appear indicating that the data was ingested
        await Page.WaitForSelectorAsync("[role='alert']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = TimeSpan.FromMinutes(5).Seconds });
        await Expect(Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();

        // Text input should be enabled again
        await Expect(Page.GetByRole(AriaRole.Textbox).First).ToBeEditableAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Build Index" })).ToBeVisibleAsync();
    }
}
