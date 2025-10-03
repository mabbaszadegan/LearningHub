using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "EducationalContents");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "EducationalContents");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "EducationalContents");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "EducationalContents");

            migrationBuilder.AddColumn<int>(
                name: "FileId",
                table: "EducationalContents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MD5Hash = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReferenceCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalContents_FileId",
                table: "EducationalContents",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_CreatedAt",
                table: "Files",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Files_FileName",
                table: "Files",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Files_MD5Hash",
                table: "Files",
                column: "MD5Hash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EducationalContents_Files_FileId",
                table: "EducationalContents",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EducationalContents_Files_FileId",
                table: "EducationalContents");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropIndex(
                name: "IX_EducationalContents_FileId",
                table: "EducationalContents");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "EducationalContents");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "EducationalContents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "EducationalContents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "EducationalContents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "EducationalContents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
