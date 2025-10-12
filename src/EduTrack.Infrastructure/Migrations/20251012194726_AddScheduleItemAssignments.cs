using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleItemAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleItemGroupAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    StudentGroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleItemGroupAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleItemGroupAssignment_ScheduleItems_ScheduleItemId",
                        column: x => x.ScheduleItemId,
                        principalTable: "ScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleItemGroupAssignment_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleItemSubChapterAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    SubChapterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleItemSubChapterAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleItemSubChapterAssignment_ScheduleItems_ScheduleItemId",
                        column: x => x.ScheduleItemId,
                        principalTable: "ScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleItemSubChapterAssignment_SubChapters_SubChapterId",
                        column: x => x.SubChapterId,
                        principalTable: "SubChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemGroupAssignment_ScheduleItemId",
                table: "ScheduleItemGroupAssignment",
                column: "ScheduleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemGroupAssignment_ScheduleItemId_StudentGroupId",
                table: "ScheduleItemGroupAssignment",
                columns: new[] { "ScheduleItemId", "StudentGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemGroupAssignment_StudentGroupId",
                table: "ScheduleItemGroupAssignment",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemSubChapterAssignment_ScheduleItemId",
                table: "ScheduleItemSubChapterAssignment",
                column: "ScheduleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemSubChapterAssignment_ScheduleItemId_SubChapterId",
                table: "ScheduleItemSubChapterAssignment",
                columns: new[] { "ScheduleItemId", "SubChapterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemSubChapterAssignment_SubChapterId",
                table: "ScheduleItemSubChapterAssignment",
                column: "SubChapterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleItemGroupAssignment");

            migrationBuilder.DropTable(
                name: "ScheduleItemSubChapterAssignment");
        }
    }
}
