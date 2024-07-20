using YouTubeGPT.AppHost;
using YouTubeGPT.Shared;

var builder = DistributedApplication.CreateBuilder(args);

var ai = builder.ExecutionContext.IsPublishMode ?
    builder.AddAzureOpenAI(ServiceNames.OpenAI)
        .AddDeployment(new(builder.Configuration["Azure:AI:ChatDeploymentName"] ?? "gpt-4o", "gpt-4o", "2024-05-13"))
        .AddDeployment(new(builder.Configuration["Azure:AI:EmbeddingDeploymentName"] ?? "text-embedding-3-small", "text-embedding-3-small", "1")) :
    builder.AddConnectionString(ServiceNames.OpenAI);


IResourceBuilder<IResourceWithConnectionString> vectorDB;
IResourceBuilder<IResourceWithConnectionString> metadataDB;
if (builder.ExecutionContext.IsPublishMode)
{

    var databaseServer = builder.AddSqlServer("sql")
        .AsAzureSqlDatabase();
    vectorDB = databaseServer.AddDatabase(ServiceNames.VectorDB);
    metadataDB = databaseServer.AddDatabase(ServiceNames.MetadataDB);
}
else
{
    vectorDB = builder.AddConnectionString(ServiceNames.VectorDB);
    metadataDB = builder.AddConnectionString(ServiceNames.MetadataDB);
}

builder.AddProject<Projects.YouTubeGPT_Ingestion>("youtubegpt-ingestion")
    .WithReference(ai)
    .WithReference(vectorDB)
    .WithReference(metadataDB)
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName")
    .WithExternalHttpEndpoints();

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
    .WithConfiguration("Azure:AI:EmbeddingDeploymentName")
    .WithConfiguration("Azure:AI:ChatDeploymentName")
    .WithExternalHttpEndpoints();

builder.Build().Run();
