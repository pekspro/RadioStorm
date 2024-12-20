﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pekspro.RadioStorm.GeneralDatabase.Models;

#pragma warning disable 219, 612, 618
#nullable disable

namespace Pekspro.RadioStorm.GeneralDatabase.CompiledModel
{
    internal partial class DownloadStateEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "Pekspro.RadioStorm.GeneralDatabase.Models.DownloadState",
                typeof(DownloadState),
                baseEntityType);

            var episodeId = runtimeEntityType.AddProperty(
                "EpisodeId",
                typeof(int),
                propertyInfo: typeof(DownloadState).GetProperty("EpisodeId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(DownloadState).GetField("<EpisodeId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd,
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: 0);
            episodeId.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                keyComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));

            var downloadStatus = runtimeEntityType.AddProperty(
                "DownloadStatus",
                typeof(DownloadState.DownloadStatusEnum),
                propertyInfo: typeof(DownloadState).GetProperty("DownloadStatus", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(DownloadState).GetField("<DownloadStatus>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            downloadStatus.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<DownloadState.DownloadStatusEnum>(
                    (DownloadState.DownloadStatusEnum v1, DownloadState.DownloadStatusEnum v2) => object.Equals((object)v1, (object)v2),
                    (DownloadState.DownloadStatusEnum v) => v.GetHashCode(),
                    (DownloadState.DownloadStatusEnum v) => v),
                keyComparer: new ValueComparer<DownloadState.DownloadStatusEnum>(
                    (DownloadState.DownloadStatusEnum v1, DownloadState.DownloadStatusEnum v2) => object.Equals((object)v1, (object)v2),
                    (DownloadState.DownloadStatusEnum v) => v.GetHashCode(),
                    (DownloadState.DownloadStatusEnum v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"),
                converter: new ValueConverter<DownloadState.DownloadStatusEnum, int>(
                    (DownloadState.DownloadStatusEnum value) => (int)value,
                    (int value) => (DownloadState.DownloadStatusEnum)value),
                jsonValueReaderWriter: new JsonConvertedValueReaderWriter<DownloadState.DownloadStatusEnum, int>(
                    JsonInt32ReaderWriter.Instance,
                    new ValueConverter<DownloadState.DownloadStatusEnum, int>(
                        (DownloadState.DownloadStatusEnum value) => (int)value,
                        (int value) => (DownloadState.DownloadStatusEnum)value)));
            downloadStatus.SetSentinelFromProviderValue(0);

            var programId = runtimeEntityType.AddProperty(
                "ProgramId",
                typeof(int),
                propertyInfo: typeof(DownloadState).GetProperty("ProgramId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(DownloadState).GetField("<ProgramId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: 0);
            programId.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                keyComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));

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
            runtimeEntityType.AddAnnotation("Relational:TableName", "DownloadState");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
