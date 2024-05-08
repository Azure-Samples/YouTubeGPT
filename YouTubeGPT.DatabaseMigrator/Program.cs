using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Shared;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<MetadataDbContext>(ServiceNames.MetadataDB);

var host = builder.Build();

var db = host.Services.GetService(typeof(MetadataDbContext)) as MetadataDbContext
         ?? throw new ArgumentException(nameof(MetadataDbContext));

db.Database.Migrate();

host.Run();
