using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using YouTubeGPT.Ingestion.Operations;

namespace YouTubeGPT.Ingestion.Components.Pages;
public partial class Home
{
    readonly BuildIndexModel model = new();
    private int progress = 0;
    private bool showProgress = false;

    [Inject]
    public required BuildVectorDatabaseOperationHandler Operation { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [Inject]
    public required ILogger<Home> Logger { get; set; }

    private async Task BuildIndexAsync()
    {
        if (string.IsNullOrEmpty(model.ChannelUrl))
        {
            Snackbar.Add("No URL entered for videos", Severity.Error);
            return;
        }

        try
        {
            progress = 0;
            showProgress = true;
            await Operation.Handle(model.ChannelUrl, new Progress<int>(p =>
            {
                progress = p;
                StateHasChanged();
            }), model.MaxVideos);
            Snackbar.Add("Index built successfully", Severity.Success);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to build index");
        }

        showProgress = false;
    }

    class BuildIndexModel
    {
        [Required]
        public string ChannelUrl { get; set; } = null!;
        [Required]
        [Range(1, 100)]
        public int MaxVideos { get; set; } = 10;
        [Required]
        [Range(1, 100)]
        public int MinDuration { get; set; } = 5;
    }
}