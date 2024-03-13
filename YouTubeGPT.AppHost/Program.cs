using Microsoft.Extensions.Hosting;
using YouTubeGPT.AppHost;
using YouTubeGPT.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var ai = builder.ExecutionContext.IsPublishMode ?
    builder.AddAzureOpenAI(ServiceNames.AzureOpenAI)
        .WithDeployment(new("gpt-35-turbo", "gpt-35-turbo", "1106"))
        .WithDeployment(new("text-embedding-ada-002", "text-embedding-ada-002", "2")) :
    builder.AddConnectionString(ServiceNames.AzureOpenAI);

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

var vectorDB = pgContainer.AddDatabase(ServiceNames.VectorDB);
var metadataDB = pgContainer.AddDatabase(ServiceNames.MetadataDB);

builder.AddProject<Projects.YouTubeGPT_Ingestion>("youtubegpt-ingestion")
    .WithReference(ai)
    .WithReference(vectorDB)
    .WithReference(metadataDB)
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName");

// We only want to launch the DB migrator when we're not in publish mode
// otherwise we'll skip it and run DB migrations as part of the CI/CD pipeline.
if (!builder.ExecutionContext.IsPublishMode)
{
    builder.AddProject<Projects.YouTubeGPT_DatabaseMigrator>("youtubegpt-databasemigrator")
        .WithReference(metadataDB);
}

builder.AddProject<Projects.YouTubeGPT_Client>("youtubegpt-client")
    .WithReference(ai)
    .WithReference(metadataDB)
    .WithReference(vectorDB)
    .WithReference(metadataDB)
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName");

builder.Build().Run();
