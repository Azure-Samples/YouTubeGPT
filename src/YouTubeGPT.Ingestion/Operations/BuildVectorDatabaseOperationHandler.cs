﻿using Microsoft.SemanticKernel.Memory;
using System.Text.Json;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YouTubeGPT.Ingestion.Models;
using YouTubeGPT.Shared;

namespace YouTubeGPT.Ingestion.Operations;

public class BuildVectorDatabaseOperationHandler(
    YoutubeClient yt,
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    ISemanticTextMemory memory,
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    ILogger<BuildVectorDatabaseOperationHandler> logger,
    MetadataDbContext metadataDbContext)
{
    public async Task Handle(string channelUrl, IProgress<int> progress, int maxVideos = 10, TimeSpan? minDuration = null)
    {
        Channel channel;
        IAsyncEnumerable<PlaylistVideo> uploads;

        try
        {
            if (channelUrl.Contains("/playlist?list="))
            {
                var playlistId = PlaylistId.Parse(channelUrl);
                var playlist = await yt.Playlists.GetAsync(playlistId);
                uploads = yt.Playlists.GetVideosAsync(playlistId);
                channel = await yt.Channels.GetAsync(playlist.Author!.ChannelId);
            }
            else
            {
                channel = await yt.Channels.GetAsync(channelUrl);
                uploads = yt.Channels.GetUploadsAsync(channel.Id);
            }
        }
        catch (ArgumentException)
        {
            logger.LogWarning("Failed to parse {ChannelUrl} as a ChannelId, trying it as a handle.", channelUrl);

            try
            {
                channel = await yt.Channels.GetByHandleAsync(channelUrl);
                uploads = yt.Channels.GetUploadsAsync(channel.Id);
            }
            catch (ArgumentException)
            {
                logger.LogError("The URL {ChannelUrl} is not in channel ID or name format. Please use one of these formats.", channelUrl);
                return;
            }
        }

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

            if (minDuration is not null && playlistVideo.Duration < minDuration)
            {
                logger.LogInformation("Skipping '{Title}' ({Url}) as the duration is below the specified threshold.", playlistVideo.Title, playlistVideo.Url);
                continue;
            }

            //logger.LogInformation("Downloading transcript for '{Title}' ({Id})", playlistVideo.Title, playlistVideo.Id);
            //var trackManifest = await yt.Videos.ClosedCaptions.GetManifestAsync(playlistVideo.Id);
            //var track = trackManifest.TryGetByLanguage("en");
            //if (track is null)
            //{
            //    logger.LogInformation("Video {Id} doesn't have an English transcript", playlistVideo.Id);
            //    continue;
            //}

            //await Console.Out.WriteLineAsync($"Downloading caption for '{playlistVideo.Title}'");
            //using var textWriter = new StringWriter();
            //await yt.Videos.ClosedCaptions.WriteToAsync(track, textWriter);

            //var captions = textWriter.ToString();
            //var key1 = await memory.SaveInformationAsync($"{channel.Id}_{Constants.CaptionsCollectionSuffix}", captions, playlistVideo.Id, additionalMetadata: additionalMetadata);

            await SaveVideoDescription(channel, playlistVideo);

            // only increment the video count if we've successfully saved the video to memory
            videoCount++;
            progress.Report((int)Math.Round(videoCount / (double)maxVideos * 100));
        }

        progress.Report(100);

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

    private async Task SaveVideoDescription(Channel channel, PlaylistVideo playlistVideo)
    {
        var video = await yt.Videos.GetAsync(playlistVideo.Id);

        var videoMetadata = new VideoMetadata()
        {
            Description = video.Description,
            Title = video.Title,
            Duration = video.Duration,
            Author = video.Author,
            Url = video.Url,
            UploadDate = video.UploadDate,
            Keywords = video.Keywords
        };

        var additionalMetadata = JsonSerializer.Serialize(videoMetadata);

        string text = $"""
                Title:
                {video.Title}

                Url:
                {video.Url}

                Description:
                {video.Description}
                """;
        var key = await memory.SaveInformationAsync($"{channel.Id}_{Constants.DescriptionsCollectionSuffix}", text, video.Id, additionalMetadata: additionalMetadata);

        logger.LogInformation("Video {VideoTitle}({VideoId}) has been saved to memory with key {Key}.", video.Title, video.Id, key);
    }
}
