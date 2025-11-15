using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class _202511150001_RemoveInteractiveLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractiveContentItems");

            migrationBuilder.DropTable(
                name: "InteractiveLessonAssignments");

            migrationBuilder.DropTable(
                name: "InteractiveLessonSubChapters");

            migrationBuilder.DropTable(
                name: "StageContentItems");

            migrationBuilder.DropTable(
                name: "InteractiveLessonStages");

            migrationBuilder.DropTable(
                name: "InteractiveLessons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractiveLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveLessons_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InteractiveContentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveLessonId = table.Column<int>(type: "int", nullable: false),
                    InteractiveQuestionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveContentItems_InteractiveLessons_InteractiveLessonId",
                        column: x => x.InteractiveLessonId,
                        principalTable: "InteractiveLessons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InteractiveContentItems_InteractiveQuestions_InteractiveQuestionId",
                        column: x => x.InteractiveQuestionId,
                        principalTable: "InteractiveQuestions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InteractiveLessonAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    InteractiveLessonId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveLessonAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveLessonAssignments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractiveLessonAssignments_InteractiveLessons_InteractiveLessonId",
                        column: x => x.InteractiveLessonId,
                        principalTable: "InteractiveLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractiveLessonStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveLessonId = table.Column<int>(type: "int", nullable: false),
                    ArrangementType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    StageType = table.Column<int>(type: "int", nullable: false),
                    TextContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveLessonStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveLessonStages_InteractiveLessons_InteractiveLessonId",
                        column: x => x.InteractiveLessonId,
                        principalTable: "InteractiveLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractiveLessonSubChapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveLessonId = table.Column<int>(type: "int", nullable: false),
                    SubChapterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveLessonSubChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveLessonSubChapters_InteractiveLessons_InteractiveLessonId",
                        column: x => x.InteractiveLessonId,
                        principalTable: "InteractiveLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractiveLessonSubChapters_SubChapters_SubChapterId",
                        column: x => x.SubChapterId,
                        principalTable: "SubChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageContentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveLessonStageId = table.Column<int>(type: "int", nullable: false),
                    InteractiveQuestionId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageContentItems_InteractiveLessonStages_InteractiveLessonStageId",
                        column: x => x.InteractiveLessonStageId,
                        principalTable: "InteractiveLessonStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageContentItems_InteractiveQuestions_InteractiveQuestionId",
                        column: x => x.InteractiveQuestionId,
                        principalTable: "InteractiveQuestions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveContentItems_InteractiveLessonId_Order",
                table: "InteractiveContentItems",
                columns: new[] { "InteractiveLessonId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveContentItems_InteractiveQuestionId",
                table: "InteractiveContentItems",
                column: "InteractiveQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveContentItems_IsActive",
                table: "InteractiveContentItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonAssignments_AssignedAt",
                table: "InteractiveLessonAssignments",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonAssignments_ClassId",
                table: "InteractiveLessonAssignments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonAssignments_InteractiveLessonId_ClassId",
                table: "InteractiveLessonAssignments",
                columns: new[] { "InteractiveLessonId", "ClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonAssignments_IsActive",
                table: "InteractiveLessonAssignments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessons_CourseId_Order",
                table: "InteractiveLessons",
                columns: new[] { "CourseId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessons_IsActive",
                table: "InteractiveLessons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonStages_ArrangementType",
                table: "InteractiveLessonStages",
                column: "ArrangementType");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonStages_InteractiveLessonId_Order",
                table: "InteractiveLessonStages",
                columns: new[] { "InteractiveLessonId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonStages_IsActive",
                table: "InteractiveLessonStages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonStages_StageType",
                table: "InteractiveLessonStages",
                column: "StageType");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonSubChapters_InteractiveLessonId_Order",
                table: "InteractiveLessonSubChapters",
                columns: new[] { "InteractiveLessonId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonSubChapters_InteractiveLessonId_SubChapterId",
                table: "InteractiveLessonSubChapters",
                columns: new[] { "InteractiveLessonId", "SubChapterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonSubChapters_IsActive",
                table: "InteractiveLessonSubChapters",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessonSubChapters_SubChapterId",
                table: "InteractiveLessonSubChapters",
                column: "SubChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_StageContentItems_InteractiveLessonStageId_Order",
                table: "StageContentItems",
                columns: new[] { "InteractiveLessonStageId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_StageContentItems_InteractiveQuestionId",
                table: "StageContentItems",
                column: "InteractiveQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StageContentItems_IsActive",
                table: "StageContentItems",
                column: "IsActive");
        }
    }
}
