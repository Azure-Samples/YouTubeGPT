using Microsoft.Playwright;
using System.Text.RegularExpressions;
using YouTubeGPT.Shared;

namespace YouTubeGPT.Tests;
public class IngestionTests : AspirePageTest
{
    [Test]
    public async Task CanIngestData()
    {
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
        await Expect(Page.GetByTestId("progress").First).ToBeVisibleAsync();

        // Wait for the alert to appear indicating that the data was ingested
        await Page.WaitForSelectorAsync("[role='alert']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible });
        await Expect(Page.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();

        // Text input should be enabled again
        await Expect(Page.GetByRole(AriaRole.Textbox).First).ToBeEditableAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Build Index" })).ToBeVisibleAsync();
    }
}
