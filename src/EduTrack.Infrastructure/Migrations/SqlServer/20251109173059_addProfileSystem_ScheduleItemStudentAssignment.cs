using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class addProfileSystem_ScheduleItemStudentAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItemStudentAssignment_AspNetUsers_StudentId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId_StudentProfileId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_StudentId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.AlterColumn<int>(
                name: "StudentProfileId",
                table: "ScheduleItemStudentAssignment",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentProfileId",
                table: "ScheduleItemStudentAssignment",
                columns: new[] { "ScheduleItemId", "StudentProfileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentProfileId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.AlterColumn<int>(
                name: "StudentProfileId",
                table: "ScheduleItemStudentAssignment",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "ScheduleItemStudentAssignment",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId",
                table: "ScheduleItemStudentAssignment",
                columns: new[] { "ScheduleItemId", "StudentId" },
                unique: true,
                filter: "[StudentProfileId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId_StudentProfileId",
                table: "ScheduleItemStudentAssignment",
                columns: new[] { "ScheduleItemId", "StudentId", "StudentProfileId" },
                unique: true,
                filter: "[StudentProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemStudentAssignment_StudentId",
                table: "ScheduleItemStudentAssignment",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItemStudentAssignment_AspNetUsers_StudentId",
                table: "ScheduleItemStudentAssignment",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
