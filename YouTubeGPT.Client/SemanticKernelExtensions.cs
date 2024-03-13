using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextGeneration;

namespace YouTubeGPT.Client;

public static class SemanticKernelExtensions
{
    public static WebApplicationBuilder AddSemanticKernel(this  WebApplicationBuilder builder)
    {
        builder.AddAzureOpenAIClient("AzureOpenAI");

        builder.Services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<OpenAIClient>();

            var chatCompletions = new AzureOpenAIChatCompletionService("gpt-35-turbo", client);
            return chatCompletions;
        });
        builder.Services.AddSingleton<IChatCompletionService>((provider) => provider.GetRequiredService<AzureOpenAIChatCompletionService>());
        builder.Services.AddSingleton<ITextGenerationService>((provider) => provider.GetRequiredService<AzureOpenAIChatCompletionService>());

        builder.Services.AddKernel();

        return builder;
    }
}
