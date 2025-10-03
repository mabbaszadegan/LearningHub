using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInteractiveLessonToCourseBasedWithNoAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveLessons_Classes_ClassId",
                table: "InteractiveLessons");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "InteractiveLessons",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_InteractiveLessons_ClassId_Order",
                table: "InteractiveLessons",
                newName: "IX_InteractiveLessons_CourseId_Order");

            migrationBuilder.CreateTable(
                name: "InteractiveLessonAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveLessonId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveLessons_Courses_CourseId",
                table: "InteractiveLessons",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveLessons_Courses_CourseId",
                table: "InteractiveLessons");

            migrationBuilder.DropTable(
                name: "InteractiveLessonAssignments");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "InteractiveLessons",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_InteractiveLessons_CourseId_Order",
                table: "InteractiveLessons",
                newName: "IX_InteractiveLessons_ClassId_Order");

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveLessons_Classes_ClassId",
                table: "InteractiveLessons",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
