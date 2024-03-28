using YouTubeGPT.Client.Plugins;

namespace YouTubeGPT.Client;

public static class SemanticKernelExtensions
{
    public static IHostApplicationBuilder AddSemanticKernelPlugins(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<CollectionSelection>();

        return builder;
    }
}
