using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class UpdateStudySessionToScheduleItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_EducationalContents_EducationalContentId",
                table: "StudySessions");

            migrationBuilder.RenameColumn(
                name: "EducationalContentId",
                table: "StudySessions",
                newName: "ScheduleItemId");

            migrationBuilder.RenameIndex(
                name: "IX_StudySessions_EducationalContentId",
                table: "StudySessions",
                newName: "IX_StudySessions_ScheduleItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_ScheduleItems_ScheduleItemId",
                table: "StudySessions",
                column: "ScheduleItemId",
                principalTable: "ScheduleItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_ScheduleItems_ScheduleItemId",
                table: "StudySessions");

            migrationBuilder.RenameColumn(
                name: "ScheduleItemId",
                table: "StudySessions",
                newName: "EducationalContentId");

            migrationBuilder.RenameIndex(
                name: "IX_StudySessions_ScheduleItemId",
                table: "StudySessions",
                newName: "IX_StudySessions_EducationalContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_EducationalContents_EducationalContentId",
                table: "StudySessions",
                column: "EducationalContentId",
                principalTable: "EducationalContents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
