using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using YouTubeGPT.Ingestion.Operations;

namespace YouTubeGPT.Ingestion.Components.Pages;
public partial class BuildIndex
{
    BuildIndexModel model = new();

    [Inject]
    public required BuildVectorDatabaseOperationHandler Operation { get; set; }

    private async Task BuildIndexAsync()
    {
        try
        {
            await Operation.Handle(model.ChannelUrl, model.MaxVideos);
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.ToString());
        }
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