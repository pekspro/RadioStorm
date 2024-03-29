﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Pekspro.RadioStorm.CacheDatabase.Models;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Pekspro.RadioStorm.CacheDatabase.CompiledModel
{
    internal sealed partial class ProgramDataEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "Pekspro.RadioStorm.CacheDatabase.Models.ProgramData",
                typeof(ProgramData),
                baseEntityType);

            var programId = runtimeEntityType.AddProperty(
                "ProgramId",
                typeof(int),
                propertyInfo: typeof(ProgramData).GetProperty("ProgramId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<ProgramId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var archived = runtimeEntityType.AddProperty(
                "Archived",
                typeof(bool),
                propertyInfo: typeof(ProgramData).GetProperty("Archived", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<Archived>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var broadcastInfo = runtimeEntityType.AddProperty(
                "BroadcastInfo",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("BroadcastInfo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<BroadcastInfo>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var categoryId = runtimeEntityType.AddProperty(
                "CategoryId",
                typeof(int),
                propertyInfo: typeof(ProgramData).GetProperty("CategoryId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<CategoryId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var categoryName = runtimeEntityType.AddProperty(
                "CategoryName",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("CategoryName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<CategoryName>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var channelId = runtimeEntityType.AddProperty(
                "ChannelId",
                typeof(int?),
                propertyInfo: typeof(ProgramData).GetProperty("ChannelId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<ChannelId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var description = runtimeEntityType.AddProperty(
                "Description",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("Description", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<Description>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var facebookPageUri = runtimeEntityType.AddProperty(
                "FacebookPageUri",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("FacebookPageUri", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<FacebookPageUri>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var hasOnDemand = runtimeEntityType.AddProperty(
                "HasOnDemand",
                typeof(bool),
                propertyInfo: typeof(ProgramData).GetProperty("HasOnDemand", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<HasOnDemand>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var hasPod = runtimeEntityType.AddProperty(
                "HasPod",
                typeof(bool),
                propertyInfo: typeof(ProgramData).GetProperty("HasPod", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<HasPod>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var latestUpdateTime = runtimeEntityType.AddProperty(
                "LatestUpdateTime",
                typeof(DateTimeOffset),
                propertyInfo: typeof(ProgramData).GetProperty("LatestUpdateTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<LatestUpdateTime>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueConverter: new DateTimeOffsetToTicksConverter());
            latestUpdateTime.AddAnnotation("Relational:ColumnType", "bigint");

            var name = runtimeEntityType.AddProperty(
                "Name",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<Name>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var programImageHighResolution = runtimeEntityType.AddProperty(
                "ProgramImageHighResolution",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("ProgramImageHighResolution", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<ProgramImageHighResolution>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var programImageLowResolution = runtimeEntityType.AddProperty(
                "ProgramImageLowResolution",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("ProgramImageLowResolution", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<ProgramImageLowResolution>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var programUri = runtimeEntityType.AddProperty(
                "ProgramUri",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("ProgramUri", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<ProgramUri>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var twitterPageUri = runtimeEntityType.AddProperty(
                "TwitterPageUri",
                typeof(string),
                propertyInfo: typeof(ProgramData).GetProperty("TwitterPageUri", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ProgramData).GetField("<TwitterPageUri>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);

            var key = runtimeEntityType.AddKey(
                new[] { programId });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ProgramData");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
