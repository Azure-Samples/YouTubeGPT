using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Shared;

namespace YouTubeGPT.Client.Plugins;

public class CollectionSelection(
    IChatCompletionService chatCompletionService,
    MetadataDbContext metadataDbContext)
{
    [KernelFunction, Description("Look for the collection that best matches the user prompt from the provided collections.")]
    public async Task<string[]> GetCollectionAsync(
        [Description("The user prompt to find a collection in")]string prompt,
        CancellationToken cancellationToken = default)
    {
        var collections =
            await metadataDbContext.Metadata
            .ToDictionaryAsync(m => m.Title, m => m.CollectionName, cancellationToken);

        ChatHistory history = [];
        history.AddSystemMessage("""
            Your job is to extract the most likely Collection value from the following list that matches the User Prompt.
            If no match can be determined, randomly select an option from the Collection values.
            Only return the Collection value, nothing else.
            """);

        history.AddUserMessage($"""
            Collections:
            {string.Join("\n- ", collections.Keys)}
            """);
        history.AddUserMessage(prompt);

        var settings = new OpenAIPromptExecutionSettings()
        {
            Temperature = 0,
            TopP = 0.1
        };

        var possibleCollection = await chatCompletionService.GetChatMessageContentAsync(
            history,
            settings,
            cancellationToken: cancellationToken);

        // There has to be a better way to do this
        //string selected = ((Azure.AI.OpenAI.ChatResponseMessage)possibleCollection.InnerContent!).Content;

        //if (collections.TryGetValue(selected, out string? value))
        //{
        //    return [$"{value}_{Constants.DescriptionsCollectionSuffix}"];
        //}

        // If no match can be determined, randomly select an option from the Collection values
        return [$"{collections.First().Value}_{Constants.DescriptionsCollectionSuffix}"];
    }
}
