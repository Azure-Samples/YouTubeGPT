using YouTubeGPT.Client.Plugins;

namespace YouTubeGPT.Client;

public static class SemanticKernelExtensions
{
    public static IHostApplicationBuilder AddSemanticKernelPlugins(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<CollectionSelection>();

        return builder;
    }
}
