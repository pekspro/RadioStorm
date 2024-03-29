﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Pekspro.RadioStorm.CacheDatabase.Models;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Pekspro.RadioStorm.CacheDatabase.CompiledModel
{
    internal sealed partial class EpisodeSongListSyncStatusDataEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "Pekspro.RadioStorm.CacheDatabase.Models.EpisodeSongListSyncStatusData",
                typeof(EpisodeSongListSyncStatusData),
                baseEntityType);

            var episodeId = runtimeEntityType.AddProperty(
                "EpisodeId",
                typeof(int),
                propertyInfo: typeof(EpisodeSongListSyncStatusData).GetProperty("EpisodeId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(EpisodeSongListSyncStatusData).GetField("<EpisodeId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var latestUpdateTime = runtimeEntityType.AddProperty(
                "LatestUpdateTime",
                typeof(DateTimeOffset),
                propertyInfo: typeof(EpisodeSongListSyncStatusData).GetProperty("LatestUpdateTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(EpisodeSongListSyncStatusData).GetField("<LatestUpdateTime>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueConverter: new DateTimeOffsetToTicksConverter());
            latestUpdateTime.AddAnnotation("Relational:ColumnType", "bigint");

            var key = runtimeEntityType.AddKey(
                new[] { episodeId });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "EpisodeSongListSyncStatusData");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
