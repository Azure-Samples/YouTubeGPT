using Microsoft.Extensions.Hosting;
using YouTubeGPT.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var ai = builder.AddAzureOpenAI("AzureOpenAI");
ai.AddDeployment("gpt-4");
ai.AddDeployment("text-embedding-ada-002");

var pgContainer = builder.AddPostgres("vector-db")
    .WithPgAdmin();

if (builder.Environment.IsDevelopment())
{
    pgContainer = pgContainer
        .WithImage("ankane/pgvector")
        .WithImageTag("latest")
        .WithVolumeMount("./data/postgres", "/var/lib/postgresql/data");
}

var vectorDB = pgContainer.AddDatabase("youtube");

builder.AddProject<Projects.YouTubeGPT_Ingestion>("youtubegpt-ingestion")
    .WithReference(ai)
    .WithReference(vectorDB)
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName");

builder.Build().Run();
