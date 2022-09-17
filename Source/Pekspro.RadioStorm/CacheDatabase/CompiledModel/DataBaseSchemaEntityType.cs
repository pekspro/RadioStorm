﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Pekspro.RadioStorm.CacheDatabase.Models;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Pekspro.RadioStorm.CacheDatabase.CompiledModel
{
    internal sealed partial class DataBaseSchemaEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "Pekspro.RadioStorm.CacheDatabase.Models.DataBaseSchema",
                typeof(DataBaseSchema),
                baseEntityType);

            var dataBaseSchemaId = runtimeEntityType.AddProperty(
                "DataBaseSchemaId",
                typeof(int),
                propertyInfo: typeof(DataBaseSchema).GetProperty("DataBaseSchemaId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(DataBaseSchema).GetField("<DataBaseSchemaId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var version = runtimeEntityType.AddProperty(
                "Version",
                typeof(int),
                propertyInfo: typeof(DataBaseSchema).GetProperty("Version", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(DataBaseSchema).GetField("<Version>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var key = runtimeEntityType.AddKey(
                new[] { dataBaseSchemaId });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "DataBaseSchema");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
