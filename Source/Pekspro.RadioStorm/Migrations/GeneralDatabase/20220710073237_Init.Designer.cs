﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pekspro.RadioStorm.GeneralDatabase;

#nullable disable

namespace Pekspro.RadioStorm.Migrations.GeneralDatabase
{
    [DbContext(typeof(GeneralDatabaseContext))]
    [Migration("20220710073237_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.6");

            modelBuilder.Entity("Pekspro.RadioStorm.Database.Models.DownloadState", b =>
                {
                    b.Property<int>("EpisodeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DownloadStatus")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProgramId")
                        .HasColumnType("INTEGER");

                    b.HasKey("EpisodeId");

                    b.ToTable("DownloadState", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
