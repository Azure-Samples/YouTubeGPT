using Microsoft.EntityFrameworkCore;
using YouTubeGPT.Ingestion.Models;

namespace YouTubeGPT.Ingestion;
public class MetadataDbContext : DbContext
{
    public DbSet<MemoryMetadata> Metadata { get; set; }
}
