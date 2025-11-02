using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class RemoveScheduleItemAssignmentModuleEducationalContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveContentItems_EducationalContents_EducationalContentId",
                table: "InteractiveContentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_StageContentItems_EducationalContents_EducationalContentId",
                table: "StageContentItems");

            migrationBuilder.DropTable(
                name: "EducationalContents");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "ScheduleItemAssignments");

            migrationBuilder.DropIndex(
                name: "IX_StageContentItems_EducationalContentId",
                table: "StageContentItems");

            migrationBuilder.DropIndex(
                name: "IX_InteractiveContentItems_EducationalContentId",
                table: "InteractiveContentItems");

            migrationBuilder.DropColumn(
                name: "EducationalContentId",
                table: "StageContentItems");

            migrationBuilder.DropColumn(
                name: "EducationalContentId",
                table: "InteractiveContentItems");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "Lessons",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EducationalContentId",
                table: "StageContentItems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EducationalContentId",
                table: "InteractiveContentItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EducationalContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<int>(type: "int", nullable: true),
                    SubChapterId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TextContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationalContents_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EducationalContents_SubChapters_SubChapterId",
                        column: x => x.SubChapterId,
                        principalTable: "SubChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleItemAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    StudentId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_StageContentItems_EducationalContentId",
                table: "StageContentItems",
                column: "EducationalContentId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveContentItems_EducationalContentId",
                table: "InteractiveContentItems",
                column: "EducationalContentId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalContents_FileId",
                table: "EducationalContents",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalContents_IsActive",
                table: "EducationalContents",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalContents_SubChapterId_Order",
                table: "EducationalContents",
                columns: new[] { "SubChapterId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalContents_Type",
                table: "EducationalContents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseId_Order",
                table: "Modules",
                columns: new[] { "CourseId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_IsActive",
                table: "Modules",
                column: "IsActive");

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

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveContentItems_EducationalContents_EducationalContentId",
                table: "InteractiveContentItems",
                column: "EducationalContentId",
                principalTable: "EducationalContents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StageContentItems_EducationalContents_EducationalContentId",
                table: "StageContentItems",
                column: "EducationalContentId",
                principalTable: "EducationalContents",
                principalColumn: "Id");
        }
    }
}
