using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddScheduleItemBlockAttemptSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleItemBlockAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    ScheduleItemType = table.Column<int>(type: "int", nullable: false),
                    BlockId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubmittedAnswerJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswerJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    PointsEarned = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxPoints = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BlockInstruction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BlockOrder = table.Column<int>(type: "int", nullable: true),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleItemBlockAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleItemBlockAttempts_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleItemBlockAttempts_ScheduleItems_ScheduleItemId",
                        column: x => x.ScheduleItemId,
                        principalTable: "ScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleItemBlockStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleItemId = table.Column<int>(type: "int", nullable: false),
                    ScheduleItemType = table.Column<int>(type: "int", nullable: false),
                    BlockId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TotalAttempts = table.Column<int>(type: "int", nullable: false),
                    CorrectAttempts = table.Column<int>(type: "int", nullable: false),
                    IncorrectAttempts = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveIncorrectAttempts = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveCorrectAttempts = table.Column<int>(type: "int", nullable: false),
                    FirstAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastCorrectAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BlockInstruction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BlockOrder = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleItemBlockStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleItemBlockStatistics_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleItemBlockStatistics_ScheduleItems_ScheduleItemId",
                        column: x => x.ScheduleItemId,
                        principalTable: "ScheduleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_AttemptedAt",
                table: "ScheduleItemBlockAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_ScheduleItemId",
                table: "ScheduleItemBlockAttempts",
                column: "ScheduleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_ScheduleItemId",
                table: "ScheduleItemBlockAttempts",
                columns: new[] { "StudentId", "ScheduleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockAttempts_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockAttempts",
                columns: new[] { "StudentId", "ScheduleItemId", "BlockId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_LastAttemptAt",
                table: "ScheduleItemBlockStatistics",
                column: "LastAttemptAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_ScheduleItemId",
                table: "ScheduleItemBlockStatistics",
                column: "ScheduleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId",
                table: "ScheduleItemBlockStatistics",
                columns: new[] { "StudentId", "ScheduleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItemBlockStatistics_StudentId_ScheduleItemId_BlockId",
                table: "ScheduleItemBlockStatistics",
                columns: new[] { "StudentId", "ScheduleItemId", "BlockId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleItemBlockAttempts");

            migrationBuilder.DropTable(
                name: "ScheduleItemBlockStatistics");
        }
    }
}
