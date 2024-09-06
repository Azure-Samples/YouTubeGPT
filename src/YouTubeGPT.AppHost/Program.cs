using Microsoft.Extensions.Hosting;
using YouTubeGPT.AppHost;
using YouTubeGPT.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var ai = builder.ExecutionContext.IsPublishMode ?
    builder.AddAzureOpenAI(ServiceNames.OpenAI)
        .AddDeployment(new(builder.Configuration["Azure:AI:ChatDeploymentName"] ?? "gpt-4o", "gpt-4o", "2024-05-13"))
        .AddDeployment(new(builder.Configuration["Azure:AI:EmbeddingDeploymentName"] ?? "text-embedding-3-small", "text-embedding-3-small", "1")) :
    bool.TryParse(builder.Configuration["UseLocalModel"], out bool c) && c == true ?
        builder.AddOllamazure(ServiceNames.OpenAI) :
        builder.AddConnectionString(ServiceNames.OpenAI);

var postgresServer = builder.AddPostgres(ServiceNames.Postgres);

if (builder.Environment.IsDevelopment())
{
    postgresServer = postgresServer
        .WithPgAdmin()
        .WithImage("pgvector/pgvector")
        .WithImageTag("pg16")
        .WithBindMount("./database", "/docker-entrypoint-initdb.d")
        // Uncomment this if you want to persist the data across app restarts. But note, if you persist the data
        // you'll need to set a password for the database, rather than using an auto-generated one.
        // $> dotnet user-secrets set "Parameters:postgres:password" "your-password"
        //.WithDataVolume()
        ;
}

if (builder.ExecutionContext.IsPublishMode)
{
    postgresServer.AsAzurePostgresFlexibleServerWithVectorSupport();
}

var vectorDB = postgresServer.AddDatabase(ServiceNames.VectorDB);
var metadataDB = postgresServer.AddDatabase(ServiceNames.MetadataDB);

builder.AddProject<Projects.YouTubeGPT_Ingestion>(ServiceNames.YouTubeGPTIngestion)
    .WithReference(ai)
    .WithReference(vectorDB)
    .WithReference(metadataDB)
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName")
    .WithExternalHttpEndpoints();

// We only want to launch the DB migrator when we're not in publish mode
// otherwise we'll skip it and run DB migrations as part of the CI/CD pipeline.
if (!builder.ExecutionContext.IsPublishMode)
{
    builder.AddProject<Projects.YouTubeGPT_DatabaseMigrator>(ServiceNames.YouTubeGPTDatabaseMigrator)
        .WithReference(metadataDB);
}

builder.AddProject<Projects.YouTubeGPT_Client>(ServiceNames.YouTubeGPTClient)
    .WithReference(ai)
    .WithReference(metadataDB)
    .WithReference(vectorDB)
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName")
    .WithConfiguration("Azure:AI:ChatDeploymentName")
    .WithExternalHttpEndpoints();

builder.Build().Run();
