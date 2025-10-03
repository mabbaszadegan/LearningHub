using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInteractiveLessonStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractiveLessonStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveLessonId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StageType = table.Column<int>(type: "int", nullable: false),
                    ArrangementType = table.Column<int>(type: "int", nullable: false),
                    TextContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EducationalContentId = table.Column<int>(type: "int", nullable: true),
                    InteractiveQuestionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageContentItems_EducationalContents_EducationalContentId",
                        column: x => x.EducationalContentId,
                        principalTable: "EducationalContents",
                        principalColumn: "Id");
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
                name: "IX_StageContentItems_EducationalContentId",
                table: "StageContentItems",
                column: "EducationalContentId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractiveLessonSubChapters");

            migrationBuilder.DropTable(
                name: "StageContentItems");

            migrationBuilder.DropTable(
                name: "InteractiveLessonStages");
        }
    }
}
