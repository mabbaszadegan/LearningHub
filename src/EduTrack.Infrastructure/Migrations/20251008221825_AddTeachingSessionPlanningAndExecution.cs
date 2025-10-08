using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeachingSessionPlanningAndExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeachingPlanProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingPlanId = table.Column<int>(type: "int", nullable: false),
                    SubTopicId = table.Column<int>(type: "int", nullable: false),
                    StudentGroupId = table.Column<int>(type: "int", nullable: false),
                    OverallStatus = table.Column<int>(type: "int", nullable: false),
                    FirstTaughtDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTaughtDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionsCount = table.Column<int>(type: "int", nullable: false),
                    OverallProgressPercentage = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingPlanProgresses", x => x.Id);
                    table.CheckConstraint("CK_OverallProgressPercentage", "OverallProgressPercentage >= 0 AND OverallProgressPercentage <= 100");
                    table.CheckConstraint("CK_OverallStatus", "OverallStatus IN (0,1,2,3,4)");
                    table.CheckConstraint("CK_SessionsCount", "SessionsCount >= 0");
                    table.ForeignKey(
                        name: "FK_TeachingPlanProgresses_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingPlanProgresses_SubChapters_SubTopicId",
                        column: x => x.SubTopicId,
                        principalTable: "SubChapters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingPlanProgresses_TeachingPlans_TeachingPlanId",
                        column: x => x.TeachingPlanId,
                        principalTable: "TeachingPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeachingSessionExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingSessionReportId = table.Column<int>(type: "int", nullable: false),
                    StudentGroupId = table.Column<int>(type: "int", nullable: false),
                    AchievedObjectives = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AchievedSubTopicsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AchievedLessonsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AdditionalTopicsCovered = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UncoveredPlannedTopics = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UncoveredReasons = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GroupFeedback = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    UnderstandingLevel = table.Column<int>(type: "int", nullable: false),
                    ParticipationLevel = table.Column<int>(type: "int", nullable: false),
                    Challenges = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NextSessionRecommendations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingSessionExecutions", x => x.Id);
                    table.CheckConstraint("CK_ParticipationLevel", "ParticipationLevel >= 1 AND ParticipationLevel <= 5");
                    table.CheckConstraint("CK_UnderstandingLevel", "UnderstandingLevel >= 1 AND UnderstandingLevel <= 5");
                    table.ForeignKey(
                        name: "FK_TeachingSessionExecutions_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingSessionExecutions_TeachingSessionReports_TeachingSessionReportId",
                        column: x => x.TeachingSessionReportId,
                        principalTable: "TeachingSessionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingSessionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingSessionReportId = table.Column<int>(type: "int", nullable: false),
                    StudentGroupId = table.Column<int>(type: "int", nullable: false),
                    PlannedObjectives = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PlannedSubTopicsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PlannedLessonsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AdditionalTopics = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PlannedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingSessionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingSessionPlans_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingSessionPlans_TeachingSessionReports_TeachingSessionReportId",
                        column: x => x.TeachingSessionReportId,
                        principalTable: "TeachingSessionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingSessionTopicCoverages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingSessionReportId = table.Column<int>(type: "int", nullable: false),
                    StudentGroupId = table.Column<int>(type: "int", nullable: false),
                    TopicType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: true),
                    TopicTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WasPlanned = table.Column<bool>(type: "bit", nullable: false),
                    WasCovered = table.Column<bool>(type: "bit", nullable: false),
                    CoveragePercentage = table.Column<int>(type: "int", nullable: false),
                    CoverageStatus = table.Column<int>(type: "int", nullable: false),
                    TeacherNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Challenges = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: true),
                    SubChapterId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingSessionTopicCoverages", x => x.Id);
                    table.CheckConstraint("CK_CoveragePercentage", "CoveragePercentage >= 0 AND CoveragePercentage <= 100");
                    table.CheckConstraint("CK_CoverageStatus", "CoverageStatus IN (0,1,2,3)");
                    table.CheckConstraint("CK_TopicType", "TopicType IN ('SubTopic', 'Lesson', 'Additional')");
                    table.ForeignKey(
                        name: "FK_TeachingSessionTopicCoverages_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeachingSessionTopicCoverages_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingSessionTopicCoverages_SubChapters_SubChapterId",
                        column: x => x.SubChapterId,
                        principalTable: "SubChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeachingSessionTopicCoverages_TeachingSessionReports_TeachingSessionReportId",
                        column: x => x.TeachingSessionReportId,
                        principalTable: "TeachingSessionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlanProgresses_FirstTaughtDate",
                table: "TeachingPlanProgresses",
                column: "FirstTaughtDate");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlanProgresses_LastTaughtDate",
                table: "TeachingPlanProgresses",
                column: "LastTaughtDate");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlanProgresses_OverallStatus",
                table: "TeachingPlanProgresses",
                column: "OverallStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlanProgresses_StudentGroupId",
                table: "TeachingPlanProgresses",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlanProgresses_SubTopicId",
                table: "TeachingPlanProgresses",
                column: "SubTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlanProgresses_TeachingPlanId_SubTopicId_StudentGroupId",
                table: "TeachingPlanProgresses",
                columns: new[] { "TeachingPlanId", "SubTopicId", "StudentGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingPlanProgresses_UpdatedAt",
                table: "TeachingPlanProgresses",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionExecutions_CompletedAt",
                table: "TeachingSessionExecutions",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionExecutions_StudentGroupId",
                table: "TeachingSessionExecutions",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionExecutions_TeachingSessionReportId_StudentGroupId",
                table: "TeachingSessionExecutions",
                columns: new[] { "TeachingSessionReportId", "StudentGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionPlans_PlannedAt",
                table: "TeachingSessionPlans",
                column: "PlannedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionPlans_StudentGroupId",
                table: "TeachingSessionPlans",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionPlans_TeachingSessionReportId_StudentGroupId",
                table: "TeachingSessionPlans",
                columns: new[] { "TeachingSessionReportId", "StudentGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionTopicCoverages_CreatedAt",
                table: "TeachingSessionTopicCoverages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionTopicCoverages_LessonId",
                table: "TeachingSessionTopicCoverages",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionTopicCoverages_StudentGroupId",
                table: "TeachingSessionTopicCoverages",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionTopicCoverages_SubChapterId",
                table: "TeachingSessionTopicCoverages",
                column: "SubChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSessionTopicCoverages_TeachingSessionReportId_StudentGroupId_TopicType_TopicId",
                table: "TeachingSessionTopicCoverages",
                columns: new[] { "TeachingSessionReportId", "StudentGroupId", "TopicType", "TopicId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeachingPlanProgresses");

            migrationBuilder.DropTable(
                name: "TeachingSessionExecutions");

            migrationBuilder.DropTable(
                name: "TeachingSessionPlans");

            migrationBuilder.DropTable(
                name: "TeachingSessionTopicCoverages");
        }
    }
}
