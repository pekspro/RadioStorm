﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable enable

namespace Pekspro.RadioStorm.GeneralDatabase.CompiledModel
{
    public partial class GeneralDatabaseContextModel
    {
        partial void Initialize()
        {
            var downloadState = DownloadStateEntityType.Create(this);

            DownloadStateEntityType.CreateAnnotations(downloadState);

            AddAnnotation("ProductVersion", "6.0.6");
        }
    }
}
