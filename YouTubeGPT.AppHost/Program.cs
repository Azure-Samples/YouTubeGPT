using Microsoft.Extensions.Hosting;
using YouTubeGPT.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var ai = builder.ExecutionContext.IsPublishMode ?
    builder.AddAzureOpenAI("AzureOpenAI") :
    builder.AddConnectionString("AzureOpenAI");

var pgContainer = builder
    .AddPostgres(
        "postgres",
        password: builder.Configuration["Aspire:Postgres:Password"]
    )
    .WithPgAdmin();

if (builder.Environment.IsDevelopment())
{
    pgContainer = pgContainer
        .WithImage("ankane/pgvector")
        .WithImageTag("latest")
        .WithBindMount("./database", "/docker-entrypoint-initdb.d")
        //.WithBindMount("./data/postgres", "/var/lib/postgresql/data")
        ;
}

var vectorDB = pgContainer.AddDatabase("vectors");
var metadataDB = pgContainer.AddDatabase("metadata");

builder.AddProject<Projects.YouTubeGPT_Ingestion>("youtubegpt-ingestion")
    .WithReference(ai)
    .WithReference(vectorDB)
    .WithReference(metadataDB)
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName");

builder.AddProject<Projects.YouTubeGPT_DatabaseMigrator>("youtubegpt-databasemigrator")
    .WithReference(metadataDB);

builder.Build().Run();
