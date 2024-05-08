using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Plugins.Memory;
using Npgsql;
using System.Text.Json;
using YouTubeGPT.Client.Plugins;
using YouTubeGPT.Ingestion;

namespace YouTubeGPT.Client.Components.Pages;
public partial class Home
{
    [Inject]
    public required Kernel Kernel { get; set; }

    [Inject]
    public required MetadataDbContext MetadataDbContext { get; set; }

    [Inject]
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public required ISemanticTextMemory Memory { get; set; }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    [Inject]
    public required CollectionSelection CollectionSelectionPlugin { get; set; }

    private IEnumerable<CollectionInfo> collectionInfos = [];
    private bool VideosAreIndexed => collectionInfos.Any();
    private string Prompt = "";

    protected override async Task OnInitializedAsync()
    {
        SetupSemanticKernel();

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

    private void SetupSemanticKernel()
    {
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        Kernel.ImportPluginFromObject(new TextMemoryPlugin(Memory));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        Kernel.ImportPluginFromObject(CollectionSelectionPlugin, nameof(CollectionSelection));
    }

    private async Task Ask()
    {
        if (string.IsNullOrEmpty(Prompt))
        {
            return;
        }

        ChatHistory history = [];
        history.AddSystemMessage($"""
            You are an AI Assistant that helps people find YouTube videos that are relevant to something they are wanting to learn about.

            You will need to look for the right collection to access using a provided function before you can perform the memory request.

            Return a summary answer to the persons enquiry, as well as links to the videos that they are interested in.
            """);

        history.AddUserMessage(Prompt);

        var chatCompletions = Kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var response = await chatCompletions.GetChatMessageContentsAsync(
            history,
            openAIPromptExecutionSettings,
            Kernel);


        //var result = await Kernel.InvokeAsync(nameof(CollectionSelection), "GetCollection", new()
        //{
        //    { "prompt", Prompt },
        //    { "collections", collectionInfos.ToDictionary(ci => ci.ChannelId, ci => ci.Title) }
        //});

        //var result = await Kernel.InvokePromptAsync("{{recall $input collection=$collection}}", new()
        //{
        //    { "input", Prompt },
        //    { "collection", "UCvtT19MZW8dq5Wwfu6B0oxw_descriptions" }
        //});
    }

    record CollectionInfo(string ChannelId, string Title);
}