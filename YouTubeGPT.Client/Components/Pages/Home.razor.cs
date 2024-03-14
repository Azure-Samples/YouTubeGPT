using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Npgsql;
using YouTubeGPT.Ingestion;

namespace YouTubeGPT.Client.Components.Pages;
public partial class Home
{
    [Inject]
    public required Kernel Kernel { get; set; }

    [Inject]
    public required MetadataDbContext MetadataDbContext { get; set; }

    [Inject]
#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public required ISemanticTextMemory Memory { get; set; }
#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    private IEnumerable<CollectionInfo> collectionInfos = [];
    private bool VideosAreIndexed => collectionInfos.Any();
    private string Prompt = "";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var collections = await Memory.GetCollectionsAsync();

            foreach (var collection in collections)
            {
                var channelId = collection.Split('_')[0];
                if (collectionInfos.Any(collectionInfos => collectionInfos.ChannelId == channelId))
                {
                    continue;
                }

                var metadata = await MetadataDbContext.Metadata.FirstAsync(md => md.ChannelId == channelId);
                collectionInfos = collectionInfos.Append(new CollectionInfo(channelId, metadata.Title));
            }
        }
        catch (PostgresException ex) 
        // Ignore exceptions raised when the database is not yet initialized
        when (ex.SqlState == "3D000")
        {
            return;
        }
    }

    private async Task Ask()
    {
        if (string.IsNullOrEmpty(Prompt))
        {
            return;
        }

#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0052 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        Kernel.ImportPluginFromObject(new TextMemoryPlugin(Memory));
#pragma warning restore SKEXP0052 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        var result = await Kernel.InvokePromptAsync("{{recall $input collection=$collection}}", new()
        {
            { "input", Prompt },
            { "collection", "UCvtT19MZW8dq5Wwfu6B0oxw_descriptions" }
        });
    }

    record CollectionInfo(string ChannelId, string Title);
}