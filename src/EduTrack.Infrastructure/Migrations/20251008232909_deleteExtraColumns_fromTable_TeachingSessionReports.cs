using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class deleteExtraColumns_fromTable_TeachingSessionReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentsJson",
                table: "TeachingSessionReports");

            migrationBuilder.DropColumn(
                name: "StatsJson",
                table: "TeachingSessionReports");

            migrationBuilder.DropColumn(
                name: "TopicsJson",
                table: "TeachingSessionReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentsJson",
                table: "TeachingSessionReports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatsJson",
                table: "TeachingSessionReports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TopicsJson",
                table: "TeachingSessionReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
