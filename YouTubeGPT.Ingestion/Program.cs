using Azure.AI.OpenAI;
using Google.Protobuf.WellKnownTypes;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Postgres;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using MudBlazor.Services;
using Npgsql;
using YoutubeExplode;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Ingestion.Components;
using YouTubeGPT.Ingestion.Operations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureOpenAI("AzureOpenAI");
builder.AddKeyedNpgsqlDataSource("vectors", null, builder => builder.UseVector());
builder.AddNpgsqlDbContext<MetadataDbContext>("metadata");

builder.Services.AddScoped(provider =>
{
    var client = provider.GetRequiredService<OpenAIClient>();
#pragma warning disable SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    ITextEmbeddingGenerationService embeddingGenerator =
        new AzureOpenAITextEmbeddingGenerationService(builder.Configuration["Azure:AI:EmbeddingDeploymentName"] ?? "text-embedding-ada-002", client);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    return embeddingGenerator;
});

#pragma warning disable SKEXP0032 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddScoped<IMemoryStore, PostgresMemoryStore>(provider =>
{
    var dataSource = provider.GetRequiredKeyedService<NpgsqlDataSource>("vectors");

    return new(dataSource, 1536);
});
#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0032 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Services.AddScoped(provider =>
{
#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    var embeddingGenerator = provider.GetRequiredService<ITextEmbeddingGenerationService>();
    var memoryStore = provider.GetRequiredService<IMemoryStore>();

    var memory = new MemoryBuilder()
    .WithMemoryStore(memoryStore)
    .WithTextEmbeddingGeneration(embeddingGenerator)
    .Build();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    return memory;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<BuildVectorDatabaseOperationHandler>();

builder.Services.AddScoped<YoutubeClient>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
