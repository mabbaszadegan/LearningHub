using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherSatisfactionToTeachingSessionExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeacherSatisfaction",
                table: "TeachingSessionExecutions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TeacherSatisfaction",
                table: "TeachingSessionExecutions",
                sql: "TeacherSatisfaction >= 1 AND TeacherSatisfaction <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TeacherSatisfaction",
                table: "TeachingSessionExecutions");

            migrationBuilder.DropColumn(
                name: "TeacherSatisfaction",
                table: "TeachingSessionExecutions");
        }
    }
}
