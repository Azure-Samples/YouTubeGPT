using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace YouTubeGPT.Client.Plugins;

public class CollectionSelection(IChatCompletionService chatCompletionService)
{
    [KernelFunction, Description("Determine the right collection to be querying for the memory request")]
    public async Task<string[]> GetCollectionAsync(
        [Description("The user prompt to find a collection in")]string prompt,
        [Description("All known collections")]IDictionary<string, string> collections,
        CancellationToken cancellationToken = default)
    {
        string metaprompt = $"""
            Your job is to extract the most likely Collection value from the following list that matches the User Prompt.
            If no match can be determined, randomly select an option from the Collection values.
            Only return the Collection value, nothing else.

            Collection:
            - {string.Join("\n- ", collections.Values)}

            User Prompt: {prompt}
            """;

        var settings = new OpenAIPromptExecutionSettings()
        {
            Temperature = 0,
            TopP = 0.1
        };

        var possibleCollection = await chatCompletionService.GetChatMessageContentAsync(
            metaprompt,
            settings,
            cancellationToken: cancellationToken);

        // There has to be a better way to do this
        string selected = ((Azure.AI.OpenAI.ChatResponseMessage)possibleCollection.InnerContent!).Content;

        return [selected];
    }
}
