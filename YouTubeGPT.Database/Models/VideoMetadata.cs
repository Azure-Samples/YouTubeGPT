using YoutubeExplode.Common;

namespace YouTubeGPT.Ingestion.Models;
internal class VideoMetadata
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TimeSpan? Duration { get; internal set; }
    public Author Author { get; internal set; } = null!;
    public string Url { get; internal set; } = null!;
    public DateTimeOffset UploadDate { get; internal set; }
    public IReadOnlyList<string> Keywords { get; internal set; } = [];
}
