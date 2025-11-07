using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class addNewStructureForStudentProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId",
                table: "CourseEnrollments");

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "CourseEnrollments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId",
                table: "CourseEnrollments",
                columns: new[] { "CourseId", "StudentId" },
                unique: true,
                filter: "[StudentProfileId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId_StudentProfileId",
                table: "CourseEnrollments",
                columns: new[] { "CourseId", "StudentId", "StudentProfileId" },
                unique: true,
                filter: "[StudentProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_StudentProfileId",
                table: "CourseEnrollments",
                column: "StudentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEnrollments_StudentProfiles_StudentProfileId",
                table: "CourseEnrollments",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseEnrollments_StudentProfiles_StudentProfileId",
                table: "CourseEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId",
                table: "CourseEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId_StudentProfileId",
                table: "CourseEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_StudentProfileId",
                table: "CourseEnrollments");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "CourseEnrollments");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId",
                table: "CourseEnrollments",
                columns: new[] { "CourseId", "StudentId" },
                unique: true);
        }
    }
}
