using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Npgsql;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Ingestion.Models;
using YouTubeGPT.Shared;

namespace YouTubeGPT.Tests;
public class IngestionTests : AspirePageTest
{
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

        // Enter the data into the page and submit form
        await Page.GetByLabel("Channel URL").ClickAsync();
        await Page.GetByLabel("Channel URL").FillAsync("https://youtube.com/@dotnet");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Build Index" }).ClickAsync();

        // Text input should be disabled
        await Expect(Page.GetByRole(AriaRole.Textbox).First).ToBeDisabledAsync();

        // Wait for the progress bar to appear
        await Page.WaitForSelectorAsync("[aria-live='polite']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await Expect(Page.GetByTestId("process")).ToBeVisibleAsync();

        // Wait for the alert to appear indicating that the data was ingested
        await Page.WaitForSelectorAsync("[role='alert']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await Expect(Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();

        // Text input should be enabled again
        await Expect(Page.GetByRole(AriaRole.Textbox).First).ToBeEditableAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Build Index" })).ToBeVisibleAsync();
    }
}
