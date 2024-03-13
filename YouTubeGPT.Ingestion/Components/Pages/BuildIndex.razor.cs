using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using YouTubeGPT.Ingestion.Operations;

namespace YouTubeGPT.Ingestion.Components.Pages;
public partial class BuildIndex
{
    BuildIndexModel model = new();
    private int progress = 0;
    private bool showProgress = false;

    [Inject]
    public required BuildVectorDatabaseOperationHandler Operation { get; set; }

    private async Task BuildIndexAsync()
    {
        try
        {
            progress = 0;
            showProgress = true;
            await Operation.Handle(model.ChannelUrl, new Progress<int>(p =>
            {
                progress = p;
                StateHasChanged();
            }), model.MaxVideos);
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.ToString());
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
    }
}