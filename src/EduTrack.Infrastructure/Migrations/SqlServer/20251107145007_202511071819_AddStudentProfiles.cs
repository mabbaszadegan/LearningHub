using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class _202511071819_AddStudentProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_AspNetUsers_StudentId",
                table: "StudySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_ScheduleItems_ScheduleItemId",
                table: "StudySessions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ScheduleItemId_StudentId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_ScheduleItemId",
                table: "ScheduleItemBlockAttempts");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockAttempts");

            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId",
                table: "CourseEnrollments");

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "Submissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "StudySessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "StudentAnswers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "ScheduleItemStudentAssignment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "ScheduleItemBlockStatistics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "ScheduleItemBlockAttempts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "CourseEnrollments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GradeLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ScheduleItemId_StudentId",
                table: "Submissions",
                columns: new[] { "ScheduleItemId", "StudentId" },
                unique: true,
                filter: "[StudentProfileId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ScheduleItemId_StudentId_StudentProfileId",
                table: "Submissions",
                columns: new[] { "ScheduleItemId", "StudentId", "StudentProfileId" },
                unique: true,
                filter: "[StudentProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_StudentProfileId",
                table: "Submissions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_StudentId_StudentProfileId_ScheduleItemId",
                table: "StudySessions",
                columns: new[] { "StudentId", "StudentProfileId", "ScheduleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_StudentProfileId",
                table: "StudySessions",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_InteractiveQuestionId_StudentId",
                table: "StudentAnswers",
                columns: new[] { "InteractiveQuestionId", "StudentId" },
                unique: true,
                filter: "[StudentProfileId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_InteractiveQuestionId_StudentId_StudentProfileId",
                table: "StudentAnswers",
                columns: new[] { "InteractiveQuestionId", "StudentId", "StudentProfileId" },
                unique: true,
                filter: "[StudentProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_StudentProfileId",
                table: "StudentAnswers",
                column: "StudentProfileId");

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
                name: "IX_ScheduleItemStudentAssignment_StudentProfileId",
                table: "ScheduleItemStudentAssignment",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockStatistics",
                columns: new[] { "StudentId", "ScheduleItemId", "BlockId" },
                unique: true,
                filter: "[StudentProfileId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_StudentProfileId_ScheduleItemId",
                table: "ScheduleItemBlockStatistics",
                columns: new[] { "StudentId", "StudentProfileId", "ScheduleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_StudentProfileId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockStatistics",
                columns: new[] { "StudentId", "StudentProfileId", "ScheduleItemId", "BlockId" },
                unique: true,
                filter: "[StudentProfileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentProfileId",
                table: "ScheduleItemBlockStatistics",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_StudentProfileId_ScheduleItemId",
                table: "ScheduleItemBlockAttempts",
                columns: new[] { "StudentId", "StudentProfileId", "ScheduleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_StudentProfileId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockAttempts",
                columns: new[] { "StudentId", "StudentProfileId", "ScheduleItemId", "BlockId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentProfileId",
                table: "ScheduleItemBlockAttempts",
                column: "StudentProfileId");

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

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_IsArchived",
                table: "StudentProfiles",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId_DisplayName",
                table: "StudentProfiles",
                columns: new[] { "UserId", "DisplayName" });

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItemBlockAttempts_StudentProfiles_StudentProfileId",
                table: "ScheduleItemBlockAttempts",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEnrollments_StudentProfiles_StudentProfileId",
                table: "CourseEnrollments",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItemBlockStatistics_StudentProfiles_StudentProfileId",
                table: "ScheduleItemBlockStatistics",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItemStudentAssignment_StudentProfiles_StudentProfileId",
                table: "ScheduleItemStudentAssignment",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentAnswers_StudentProfiles_StudentProfileId",
                table: "StudentAnswers",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_AspNetUsers_StudentId",
                table: "StudySessions",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_ScheduleItems_ScheduleItemId",
                table: "StudySessions",
                column: "ScheduleItemId",
                principalTable: "ScheduleItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_StudentProfiles_StudentProfileId",
                table: "StudySessions",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_StudentProfiles_StudentProfileId",
                table: "Submissions",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItemBlockAttempts_StudentProfiles_StudentProfileId",
                table: "ScheduleItemBlockAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseEnrollments_StudentProfiles_StudentProfileId",
                table: "CourseEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItemBlockStatistics_StudentProfiles_StudentProfileId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItemStudentAssignment_StudentProfiles_StudentProfileId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentAnswers_StudentProfiles_StudentProfileId",
                table: "StudentAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_AspNetUsers_StudentId",
                table: "StudySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_ScheduleItems_ScheduleItemId",
                table: "StudySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_StudentProfiles_StudentProfileId",
                table: "StudySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_StudentProfiles_StudentProfileId",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ScheduleItemId_StudentId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ScheduleItemId_StudentId_StudentProfileId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_StudentProfileId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_StudySessions_StudentId_StudentProfileId_ScheduleItemId",
                table: "StudySessions");

            migrationBuilder.DropIndex(
                name: "IX_StudySessions_StudentProfileId",
                table: "StudySessions");

            migrationBuilder.DropIndex(
                name: "IX_StudentAnswers_InteractiveQuestionId_StudentId",
                table: "StudentAnswers");

            migrationBuilder.DropIndex(
                name: "IX_StudentAnswers_InteractiveQuestionId_StudentId_StudentProfileId",
                table: "StudentAnswers");

            migrationBuilder.DropIndex(
                name: "IX_StudentAnswers_StudentProfileId",
                table: "StudentAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId_StudentProfileId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemStudentAssignment_StudentProfileId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_StudentProfileId_ScheduleItemId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_StudentProfileId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentProfileId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_StudentProfileId_ScheduleItemId",
                table: "ScheduleItemBlockAttempts");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_StudentProfileId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockAttempts");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentProfileId",
                table: "ScheduleItemBlockAttempts");

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
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "StudentAnswers");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "ScheduleItemStudentAssignment");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "ScheduleItemBlockStatistics");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "ScheduleItemBlockAttempts");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "CourseEnrollments");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ScheduleItemId_StudentId",
                table: "Submissions",
                columns: new[] { "ScheduleItemId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId",
                table: "ScheduleItemStudentAssignment",
                columns: new[] { "ScheduleItemId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId",
                table: "ScheduleItemBlockStatistics",
                columns: new[] { "StudentId", "ScheduleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockStatistics",
                columns: new[] { "StudentId", "ScheduleItemId", "BlockId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_ScheduleItemId",
                table: "ScheduleItemBlockAttempts",
                columns: new[] { "StudentId", "ScheduleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockAttempts",
                columns: new[] { "StudentId", "ScheduleItemId", "BlockId" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId_StudentId",
                table: "CourseEnrollments",
                columns: new[] { "CourseId", "StudentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_AspNetUsers_StudentId",
                table: "StudySessions",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_ScheduleItems_ScheduleItemId",
                table: "StudySessions",
                column: "ScheduleItemId",
                principalTable: "ScheduleItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
