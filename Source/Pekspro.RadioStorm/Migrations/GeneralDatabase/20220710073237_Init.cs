using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pekspro.RadioStorm.Migrations.GeneralDatabase
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadState",
                columns: table => new
                {
                    EpisodeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProgramId = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadStatus = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadState", x => x.EpisodeId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadState");
        }
    }
}
