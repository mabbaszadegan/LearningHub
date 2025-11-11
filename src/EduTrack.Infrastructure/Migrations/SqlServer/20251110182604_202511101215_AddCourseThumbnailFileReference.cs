using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class _202511101215_AddCourseThumbnailFileReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThumbnailFileId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ThumbnailFileId",
                table: "Courses",
                column: "ThumbnailFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Files_ThumbnailFileId",
                table: "Courses",
                column: "ThumbnailFileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Files_ThumbnailFileId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ThumbnailFileId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ThumbnailFileId",
                table: "Courses");
        }
    }
}
