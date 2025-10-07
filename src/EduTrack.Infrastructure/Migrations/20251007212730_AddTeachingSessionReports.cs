using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeachingSessionReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionReportId",
                table: "ScheduleItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduleItemAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleItemAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleItemAssignments_ScheduleItems_ScheduleItemId",
                        column: x => x.ScheduleItemId,
                        principalTable: "ScheduleItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeachingSessionReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingPlanId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TopicsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    StatsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AttachmentsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedByTeacherId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingSessionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingSessionReports_TeachingPlans_TeachingPlanId",
                        column: x => x.TeachingPlanId,
                        principalTable: "TeachingPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeachingSessionAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingSessionReportId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ParticipationScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingSessionAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingSessionAttendances_TeachingSessionReports_TeachingSessionReportId",
                        column: x => x.TeachingSessionReportId,
                        principalTable: "TeachingSessionReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItems_SessionReportId",
                table: "ScheduleItems",
                column: "SessionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemAssignments_GroupId",
                table: "ScheduleItemAssignments",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemAssignments_ScheduleItemId_StudentId_GroupId",
                table: "ScheduleItemAssignments",
                columns: new[] { "ScheduleItemId", "StudentId", "GroupId" },
                unique: true,
                filter: "[StudentId] IS NOT NULL AND [GroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemAssignments_StudentId",
                table: "ScheduleItemAssignments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionAttendances_Status",
                table: "TeachingSessionAttendances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionAttendances_StudentId",
                table: "TeachingSessionAttendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionAttendances_TeachingSessionReportId_StudentId",
                table: "TeachingSessionAttendances",
                columns: new[] { "TeachingSessionReportId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionReports_CreatedByTeacherId",
                table: "TeachingSessionReports",
                column: "CreatedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionReports_SessionDate",
                table: "TeachingSessionReports",
                column: "SessionDate");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionReports_TeachingPlanId",
                table: "TeachingSessionReports",
                column: "TeachingPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_TeachingSessionReports_SessionReportId",
                table: "ScheduleItems",
                column: "SessionReportId",
                principalTable: "TeachingSessionReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_TeachingSessionReports_SessionReportId",
                table: "ScheduleItems");

            migrationBuilder.DropTable(
                name: "ScheduleItemAssignments");

            migrationBuilder.DropTable(
                name: "TeachingSessionAttendances");

            migrationBuilder.DropTable(
                name: "TeachingSessionReports");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItems_SessionReportId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "SessionReportId",
                table: "ScheduleItems");
        }
    }
}
