using Microsoft.EntityFrameworkCore;
using YouTubeGPT.Ingestion.Models;

namespace YouTubeGPT.Ingestion;
public class MetadataDbContext(DbContextOptions<MetadataDbContext> options) : DbContext(options)
{
    public DbSet<MemoryMetadata> Metadata { get; set; }
}
