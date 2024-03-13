using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using YouTubeGPT.Ingestion;

var host = Host.CreateApplicationBuilder(args).Build();

var db = host.Services.GetService(typeof(MetadataDbContext)) as MetadataDbContext
         ?? throw new ArgumentException();

db.Database.Migrate();

host.Run();