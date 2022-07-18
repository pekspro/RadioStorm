using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pekspro.RadioStorm.Migrations.CacheDatabase
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelData",
                columns: table => new
                {
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelGroupName = table.Column<string>(type: "TEXT", nullable: false),
                    ChannelImageLowResolution = table.Column<string>(type: "TEXT", nullable: true),
                    ChannelImageHighResolution = table.Column<string>(type: "TEXT", nullable: true),
                    ChannelColor = table.Column<string>(type: "TEXT", nullable: true),
                    LiveAudioUrl = table.Column<string>(type: "TEXT", nullable: true),
                    WebPageUri = table.Column<string>(type: "TEXT", nullable: true),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelData", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSongListItemData",
                columns: table => new
                {
                    ChannelSongListItemDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Artist = table.Column<string>(type: "TEXT", nullable: true),
                    AlbumName = table.Column<string>(type: "TEXT", nullable: true),
                    Composer = table.Column<string>(type: "TEXT", nullable: true),
                    PublishDate = table.Column<long>(type: "bigint", nullable: false),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSongListItemData", x => x.ChannelSongListItemDataId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSongListSyncStatusData",
                columns: table => new
                {
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSongListSyncStatusData", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelStatusData",
                columns: table => new
                {
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentProgramId = table.Column<int>(type: "INTEGER", nullable: true),
                    CurrentProgram = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentProgramImage = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentProgramDescription = table.Column<string>(type: "TEXT", nullable: true),
                    CurrentStartTime = table.Column<long>(type: "bigint", nullable: true),
                    CurrentEndTime = table.Column<long>(type: "bigint", nullable: true),
                    NextProgramId = table.Column<int>(type: "INTEGER", nullable: true),
                    NextProgram = table.Column<string>(type: "TEXT", nullable: true),
                    NextProgramImage = table.Column<string>(type: "TEXT", nullable: true),
                    NextProgramDescription = table.Column<string>(type: "TEXT", nullable: true),
                    NextStartTime = table.Column<long>(type: "bigint", nullable: true),
                    NextEndTime = table.Column<long>(type: "bigint", nullable: true),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelStatusData", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "DataBaseSchema",
                columns: table => new
                {
                    DataBaseSchemaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBaseSchema", x => x.DataBaseSchemaId);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeData",
                columns: table => new
                {
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgramId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProgramName = table.Column<string>(type: "TEXT", nullable: true),
                    AudioStreamWithMusicUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AudioStreamWithoutMusicUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AudioDownloadUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AudioStreamWithMusicDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioStreamWithoutMusicDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioDownloadDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    PublishDate = table.Column<long>(type: "bigint", nullable: false),
                    AudioStreamWithMusicExpireDate = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    EpisodeImage = table.Column<string>(type: "TEXT", nullable: true),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeData", x => x.EpisodeId);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeListSyncStatusData",
                columns: table => new
                {
                    ProgramId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IncrementallyUpdateCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false),
                    LatestFullSynchronizingTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeListSyncStatusData", x => x.ProgramId);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeSongListItemData",
                columns: table => new
                {
                    EpisodeSongListItemDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Artist = table.Column<string>(type: "TEXT", nullable: true),
                    AlbumName = table.Column<string>(type: "TEXT", nullable: true),
                    Composer = table.Column<string>(type: "TEXT", nullable: true),
                    PublishDate = table.Column<long>(type: "bigint", nullable: false),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeSongListItemData", x => x.EpisodeSongListItemDataId);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeSongListSyncStatusData",
                columns: table => new
                {
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeSongListSyncStatusData", x => x.EpisodeId);
                });

            migrationBuilder.CreateTable(
                name: "ListSyncStatusData",
                columns: table => new
                {
                    TypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListSyncStatusData", x => x.TypeId);
                });

            migrationBuilder.CreateTable(
                name: "ProgramData",
                columns: table => new
                {
                    ProgramId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryName = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ProgramImageHighResolution = table.Column<string>(type: "TEXT", nullable: true),
                    ProgramImageLowResolution = table.Column<string>(type: "TEXT", nullable: true),
                    Archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasOnDemand = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasPod = table.Column<bool>(type: "INTEGER", nullable: false),
                    BroadcastInfo = table.Column<string>(type: "TEXT", nullable: true),
                    ProgramUri = table.Column<string>(type: "TEXT", nullable: true),
                    FacebookPageUri = table.Column<string>(type: "TEXT", nullable: true),
                    TwitterPageUri = table.Column<string>(type: "TEXT", nullable: true),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: true),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramData", x => x.ProgramId);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEpisodeListItemData",
                columns: table => new
                {
                    ScheduledEpisodeDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<long>(type: "bigint", nullable: false),
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    StartTimeUtc = table.Column<long>(type: "bigint", nullable: false),
                    EndTimeUtc = table.Column<long>(type: "bigint", nullable: false),
                    ProgramId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProgramName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEpisodeListItemData", x => x.ScheduledEpisodeDataId);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledEpisodeListSyncStatusData",
                columns: table => new
                {
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<long>(type: "bigint", nullable: false),
                    LatestUpdateTime = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledEpisodeListSyncStatusData", x => x.ChannelId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSongListItemData_ChannelId_PublishDate",
                table: "ChannelSongListItemData",
                columns: new[] { "ChannelId", "PublishDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeData_ProgramId_PublishDate",
                table: "EpisodeData",
                columns: new[] { "ProgramId", "PublishDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeSongListItemData_EpisodeId_PublishDate",
                table: "EpisodeSongListItemData",
                columns: new[] { "EpisodeId", "PublishDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEpisodeListItemData_ChannelId_Date",
                table: "ScheduledEpisodeListItemData",
                columns: new[] { "ChannelId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEpisodeListSyncStatusData_ChannelId",
                table: "ScheduledEpisodeListSyncStatusData",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledEpisodeListSyncStatusData_Date",
                table: "ScheduledEpisodeListSyncStatusData",
                column: "Date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelData");

            migrationBuilder.DropTable(
                name: "ChannelSongListItemData");

            migrationBuilder.DropTable(
                name: "ChannelSongListSyncStatusData");

            migrationBuilder.DropTable(
                name: "ChannelStatusData");

            migrationBuilder.DropTable(
                name: "DataBaseSchema");

            migrationBuilder.DropTable(
                name: "EpisodeData");

            migrationBuilder.DropTable(
                name: "EpisodeListSyncStatusData");

            migrationBuilder.DropTable(
                name: "EpisodeSongListItemData");

            migrationBuilder.DropTable(
                name: "EpisodeSongListSyncStatusData");

            migrationBuilder.DropTable(
                name: "ListSyncStatusData");

            migrationBuilder.DropTable(
                name: "ProgramData");

            migrationBuilder.DropTable(
                name: "ScheduledEpisodeListItemData");

            migrationBuilder.DropTable(
                name: "ScheduledEpisodeListSyncStatusData");
        }
    }
}
