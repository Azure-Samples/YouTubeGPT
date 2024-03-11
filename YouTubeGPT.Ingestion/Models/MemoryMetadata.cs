namespace YouTubeGPT.Ingestion.Models;
public class MemoryMetadata
{
    public int Id { get; set; }
    public string CollectionName { get; set; } = null!;
    public string ChannelId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string ChannelUrl { get; set; } = null!;
}
