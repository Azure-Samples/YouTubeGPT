using YoutubeExplode.Common;

namespace YouTubeGPT.Ingestion.Models;
public class VideoMetadata
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TimeSpan? Duration { get; set; }
    public Author Author { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTimeOffset UploadDate { get; set; }
    public IReadOnlyList<string> Keywords { get; set; } = [];
}
