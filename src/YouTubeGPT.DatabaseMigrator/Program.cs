using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Shared;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<MetadataDbContext>(ServiceNames.MetadataDB);

var host = builder.Build();

var db = host.Services.GetService<MetadataDbContext>()
         ?? throw new ArgumentException(nameof(MetadataDbContext));

var connection = db.Database.GetDbConnection();
var logger = host.Services.GetService<ILogger<Program>>()!;

var isDbReady = false;
var retryCount = 10;
var tryCount = 0;

while (!isDbReady && tryCount++ < retryCount)
{
    try
    {
        connection.Open();
        isDbReady = true;
        logger.LogInformation("Database connection opened successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning("Failed to open database connection {Exception}", ex);
        await Task.Delay(1000);
    }
}

if (!isDbReady)
{
    logger.LogError("Failed to open database connection after {RetryCount} retries", retryCount);
    return;
}

db.Database.Migrate();
logger.LogInformation("Database migration completed successfully");

host.Run();
