﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Pekspro.RadioStorm.CacheDatabase.Models;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Pekspro.RadioStorm.CacheDatabase.CompiledModel
{
    internal sealed partial class ChannelDataEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "Pekspro.RadioStorm.CacheDatabase.Models.ChannelData",
                typeof(ChannelData),
                baseEntityType);

            var channelId = runtimeEntityType.AddProperty(
                "ChannelId",
                typeof(int),
                propertyInfo: typeof(ChannelData).GetProperty("ChannelId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<ChannelId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var channelColor = runtimeEntityType.AddProperty(
                "ChannelColor",
                typeof(string),
                propertyInfo: typeof(ChannelData).GetProperty("ChannelColor", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<ChannelColor>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var channelGroupName = runtimeEntityType.AddProperty(
                "ChannelGroupName",
                typeof(string),
                propertyInfo: typeof(ChannelData).GetProperty("ChannelGroupName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<ChannelGroupName>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var channelImageHighResolution = runtimeEntityType.AddProperty(
                "ChannelImageHighResolution",
                typeof(string),
                propertyInfo: typeof(ChannelData).GetProperty("ChannelImageHighResolution", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<ChannelImageHighResolution>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var channelImageLowResolution = runtimeEntityType.AddProperty(
                "ChannelImageLowResolution",
                typeof(string),
                propertyInfo: typeof(ChannelData).GetProperty("ChannelImageLowResolution", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<ChannelImageLowResolution>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var latestUpdateTime = runtimeEntityType.AddProperty(
                "LatestUpdateTime",
                typeof(DateTimeOffset),
                propertyInfo: typeof(ChannelData).GetProperty("LatestUpdateTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<LatestUpdateTime>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueConverter: new DateTimeOffsetToTicksConverter());
            latestUpdateTime.AddAnnotation("Relational:ColumnType", "bigint");

            var liveAudioUrl = runtimeEntityType.AddProperty(
                "LiveAudioUrl",
                typeof(string),
                propertyInfo: typeof(ChannelData).GetProperty("LiveAudioUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<LiveAudioUrl>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var title = runtimeEntityType.AddProperty(
                "Title",
                typeof(string),
                propertyInfo: typeof(ChannelData).GetProperty("Title", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<Title>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var webPageUri = runtimeEntityType.AddProperty(
                "WebPageUri",
                typeof(string),
                propertyInfo: typeof(ChannelData).GetProperty("WebPageUri", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ChannelData).GetField("<WebPageUri>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var key = runtimeEntityType.AddKey(
                new[] { channelId });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ChannelData");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
