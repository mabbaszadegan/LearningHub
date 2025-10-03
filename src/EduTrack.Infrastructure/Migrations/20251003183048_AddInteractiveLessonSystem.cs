using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInteractiveLessonSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractiveLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveLessons_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractiveQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ImageFileId = table.Column<int>(type: "int", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveQuestions_Files_ImageFileId",
                        column: x => x.ImageFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InteractiveContentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveLessonId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EducationalContentId = table.Column<int>(type: "int", nullable: true),
                    InteractiveQuestionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveContentItems_EducationalContents_EducationalContentId",
                        column: x => x.EducationalContentId,
                        principalTable: "EducationalContents",
                        principalColumn: "Id");
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
                name: "QuestionChoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveQuestionId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionChoices_InteractiveQuestions_InteractiveQuestionId",
                        column: x => x.InteractiveQuestionId,
                        principalTable: "InteractiveQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InteractiveQuestionId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SelectedChoiceId = table.Column<int>(type: "int", nullable: true),
                    BooleanAnswer = table.Column<bool>(type: "bit", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    AnsweredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GradedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_InteractiveQuestions_InteractiveQuestionId",
                        column: x => x.InteractiveQuestionId,
                        principalTable: "InteractiveQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_QuestionChoices_SelectedChoiceId",
                        column: x => x.SelectedChoiceId,
                        principalTable: "QuestionChoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveContentItems_EducationalContentId",
                table: "InteractiveContentItems",
                column: "EducationalContentId");

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
                name: "IX_InteractiveLessons_ClassId_Order",
                table: "InteractiveLessons",
                columns: new[] { "ClassId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveLessons_IsActive",
                table: "InteractiveLessons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveQuestions_ImageFileId",
                table: "InteractiveQuestions",
                column: "ImageFileId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveQuestions_IsActive",
                table: "InteractiveQuestions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveQuestions_Type",
                table: "InteractiveQuestions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionChoices_InteractiveQuestionId_Order",
                table: "QuestionChoices",
                columns: new[] { "InteractiveQuestionId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_AnsweredAt",
                table: "StudentAnswers",
                column: "AnsweredAt");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_InteractiveQuestionId",
                table: "StudentAnswers",
                column: "InteractiveQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_SelectedChoiceId",
                table: "StudentAnswers",
                column: "SelectedChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_StudentId",
                table: "StudentAnswers",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractiveContentItems");

            migrationBuilder.DropTable(
                name: "StudentAnswers");

            migrationBuilder.DropTable(
                name: "InteractiveLessons");

            migrationBuilder.DropTable(
                name: "QuestionChoices");

            migrationBuilder.DropTable(
                name: "InteractiveQuestions");
        }
    }
}
