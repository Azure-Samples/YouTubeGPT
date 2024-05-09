using Microsoft.SemanticKernel.Plugins.Memory;
using YouTubeGPT.Client.Plugins;

namespace YouTubeGPT.Client;

public static class SemanticKernelExtensions
{
    public static IHostApplicationBuilder AddSemanticKernelPlugins(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<CollectionSelection>();

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddTransient<TextMemoryPlugin>();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        return builder;
    }
}
