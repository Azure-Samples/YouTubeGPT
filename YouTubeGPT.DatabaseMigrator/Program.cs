using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using YouTubeGPT.Ingestion;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<MetadataDbContext>("metadata");

var host = builder.Build();

var db = host.Services.GetService(typeof(MetadataDbContext)) as MetadataDbContext
         ?? throw new ArgumentException();

db.Database.Migrate();

host.Run();
