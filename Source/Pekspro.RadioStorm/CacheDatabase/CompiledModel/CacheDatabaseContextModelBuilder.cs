﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Pekspro.RadioStorm.CacheDatabase.CompiledModel
{
    public sealed partial class CacheDatabaseContextModel
    {
        partial void Initialize()
        {
            var channelData = ChannelDataEntityType.Create(this);
            var channelSongListItemData = ChannelSongListItemDataEntityType.Create(this);
            var channelSongListSyncStatusData = ChannelSongListSyncStatusDataEntityType.Create(this);
            var channelStatusData = ChannelStatusDataEntityType.Create(this);
            var dataBaseSchema = DataBaseSchemaEntityType.Create(this);
            var episodeData = EpisodeDataEntityType.Create(this);
            var episodeListSyncStatusData = EpisodeListSyncStatusDataEntityType.Create(this);
            var episodeSongListItemData = EpisodeSongListItemDataEntityType.Create(this);
            var episodeSongListSyncStatusData = EpisodeSongListSyncStatusDataEntityType.Create(this);
            var listSyncStatusData = ListSyncStatusDataEntityType.Create(this);
            var programData = ProgramDataEntityType.Create(this);
            var scheduledEpisodeListItemData = ScheduledEpisodeListItemDataEntityType.Create(this);
            var scheduledEpisodeListSyncStatusData = ScheduledEpisodeListSyncStatusDataEntityType.Create(this);

            ChannelDataEntityType.CreateAnnotations(channelData);
            ChannelSongListItemDataEntityType.CreateAnnotations(channelSongListItemData);
            ChannelSongListSyncStatusDataEntityType.CreateAnnotations(channelSongListSyncStatusData);
            ChannelStatusDataEntityType.CreateAnnotations(channelStatusData);
            DataBaseSchemaEntityType.CreateAnnotations(dataBaseSchema);
            EpisodeDataEntityType.CreateAnnotations(episodeData);
            EpisodeListSyncStatusDataEntityType.CreateAnnotations(episodeListSyncStatusData);
            EpisodeSongListItemDataEntityType.CreateAnnotations(episodeSongListItemData);
            EpisodeSongListSyncStatusDataEntityType.CreateAnnotations(episodeSongListSyncStatusData);
            ListSyncStatusDataEntityType.CreateAnnotations(listSyncStatusData);
            ProgramDataEntityType.CreateAnnotations(programData);
            ScheduledEpisodeListItemDataEntityType.CreateAnnotations(scheduledEpisodeListItemData);
            ScheduledEpisodeListSyncStatusDataEntityType.CreateAnnotations(scheduledEpisodeListSyncStatusData);

            AddAnnotation("ProductVersion", "6.0.6");
        }
    }
}
