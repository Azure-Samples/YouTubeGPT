using Microsoft.SemanticKernel.Memory;
using Spectre.Console;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YouTubeGPT.Ingestion.Models;

namespace YouTubeGPT.Ingestion.Operations;

internal class BuildVectorDatabaseOperationHandler(
    YoutubeClient yt,
#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    ISemanticTextMemory memory,
#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    ILogger<BuildVectorDatabaseOperationHandler> logger,
    MetadataDbContext metadataDbContext)
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

        int maxVideos = AnsiConsole.Ask("Enter the maximum number of videos to process (10): ", 10);
        int videoCount = 0;

        await foreach (var playlistVideo in uploads)
        {
            if (videoCount == maxVideos)
            {
                break;
            }

            if (playlistVideo.Duration is null)
            {
                logger.LogInformation("Skipping '{Title}' ({Url}) as there is no duration, so it's probably upcoming", playlistVideo.Title, playlistVideo.Url);
                continue;
            }

            logger.LogInformation("Downloading transcript for '{Title}' ({Id})", playlistVideo.Title, playlistVideo.Id);
            var trackManifest = await yt.Videos.ClosedCaptions.GetManifestAsync(playlistVideo.Id);
            var track = trackManifest.TryGetByLanguage("en");
            if (track is null)
            {
                logger.LogInformation("Video {Id} doesn't have an English transcript", playlistVideo.Id);
                continue;
            }

            await Console.Out.WriteLineAsync($"Downloading caption for '{playlistVideo.Title}'");
            using var textWriter = new StringWriter();
            await yt.Videos.ClosedCaptions.WriteToAsync(track, textWriter, new ConsoleProgress());

            var captions = textWriter.ToString();

            var key1 = await memory.SaveInformationAsync($"{channel.Id}_{Constants.CaptionsCollectionSuffix}", captions, playlistVideo.Id);

            var video = await yt.Videos.GetAsync(playlistVideo.Id);
            var key2 = await memory.SaveInformationAsync($"{channel.Id}_{Constants.DescriptionsCollectionSuffix}", video.Description, video.Id);

            await Console.Out.WriteLineAsync($"Video '{video.Title}' has been saved to memory.");
            logger.LogInformation("Video {VideoTitle}({VideoId}) has been saved to memory", video.Title, video.Id);

            // only increment the video count if we've successfully saved the video to memory
            videoCount++;
        }

        if (videoCount > 0)
        {
            metadataDbContext.Metadata.Add(new MemoryMetadata
            {
                ChannelId = channel.Id,
                ChannelUrl = channel.Url,
                CollectionName = channel.Id,
                Title = channel.Title,
            });
            await metadataDbContext.SaveChangesAsync();
        }
    }
}
