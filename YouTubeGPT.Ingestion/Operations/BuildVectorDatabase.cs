using Spectre.Console;
using YoutubeExplode;
using YoutubeExplode.Channels;

namespace YouTubeGPT.Ingestion.Operations;

internal class BuildVectorDatabaseOperationHandler(
    YoutubeClient yt,
    ILogger<BuildVectorDatabaseOperationHandler> logger)
{
    public async Task Handle()
    {
        var channelUrl = AnsiConsole.Ask<string>("Enter YouTube channel URL: ");

        Channel channel;

        try
        {
            channel = await yt.Channels.GetAsync(channelUrl);
        }
        catch (ArgumentException)
        {
            logger.LogWarning("Failed to parse {ChannelUrl} as a ChannelId, trying it as a handle", channelUrl);

            try
            {
                channel = await yt.Channels.GetByHandleAsync(channelUrl);
            }
            catch (ArgumentException)
            {
                logger.LogError("Failed to parse {ChannelUrl} as a ChannelHandle, aborting.", channelUrl);

                await Console.Out.WriteLineAsync("The URL provided is not in channel ID or name format. Please use one of these formats.");
                return;
            }
        }

        var uploads = yt.Channels.GetUploadsAsync(channel.Id);

        await foreach (var video in uploads)
        {
            if (video.Duration is null)
            {
                logger.LogInformation("Skipping '{Title}' ({Url}) as there is no duration, so it's probably upcoming", video.Title, video.Url);
                continue;
            }

            logger.LogInformation("Downloading transcript for '{Title}' ({Id})", video.Title, video.Id);
            var trackManifest = await yt.Videos.ClosedCaptions.GetManifestAsync(video.Id);
            var track = trackManifest.TryGetByLanguage("en");
            if (track is null)
            {
                logger.LogInformation("Video {Id} doesn't have an English transcript", video.Id);
                continue;
            }

            await Console.Out.WriteLineAsync($"Downloading caption for '{video.Title}'");
            using var textWriter = new StringWriter();
            await yt.Videos.ClosedCaptions.WriteToAsync(track, textWriter, new ConsoleProgress());

            var captions = textWriter.ToString();

            await Console.Out.WriteLineAsync(captions);
        }
    }
}
