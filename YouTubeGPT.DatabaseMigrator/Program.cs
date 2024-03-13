using Microsoft.Extensions.Hosting;
using YouTubeGPT.Ingestion;

var host = Host.CreateApplicationBuilder(args).Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MetadataDbContext>();
    db.Database.Migrate();
}

host.Run();