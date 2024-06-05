using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Postgres;
using Microsoft.SemanticKernel.Memory;
using Npgsql;

namespace YouTubeGPT.Shared;

public static class SemanticKernelExtensions
{
    private const int VectorSize = 1536;

    public static IHostApplicationBuilder AddSemanticKernel(this IHostApplicationBuilder builder)
    {
        builder.AddAzureOpenAIClient(ServiceNames.OpenAI);
        builder.Services.AddAzureOpenAIChatCompletion(builder.Configuration["Azure:AI:ChatDeploymentName"] ?? "gpt-4o");
        builder.Services.AddKernel();
        return builder;
    }

    public static IHostApplicationBuilder AddSemanticKernelMemory(this IHostApplicationBuilder builder)
    {
        builder.AddKeyedNpgsqlDataSource(ServiceNames.VectorDB, null, builder => builder.UseVector());

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddAzureOpenAITextEmbeddingGeneration(builder.Configuration["Azure:AI:EmbeddingDeploymentName"] ?? "text-embedding-3-small");
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddSingleton<IMemoryStore, PostgresMemoryStore>(provider =>
        {
            var dataSource = provider.GetRequiredKeyedService<NpgsqlDataSource>(ServiceNames.VectorDB);

            return new(dataSource, VectorSize);
        });
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddSingleton<ISemanticTextMemory, SemanticTextMemory>();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        return builder;
    }
}
