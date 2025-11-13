using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class _202511130945_AddScheduleItemContexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TeachingPlanId",
                table: "ScheduleItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "ScheduleItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItems_CourseId",
                table: "ScheduleItems",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_Courses_CourseId",
                table: "ScheduleItems",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_Courses_CourseId",
                table: "ScheduleItems");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItems_CourseId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "ScheduleItems");

            migrationBuilder.AlterColumn<int>(
                name: "TeachingPlanId",
                table: "ScheduleItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
