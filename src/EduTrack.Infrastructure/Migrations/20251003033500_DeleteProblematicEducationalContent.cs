using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteProblematicEducationalContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete all EducationalContent records that have FileId references to non-existent Files
            migrationBuilder.Sql(@"
                DELETE FROM EducationalContents 
                WHERE FileId IS NOT NULL 
                AND FileId NOT IN (SELECT Id FROM Files);
            ");

            // Also delete any EducationalContent records with invalid FileId values
            migrationBuilder.Sql(@"
                DELETE FROM EducationalContents 
                WHERE FileId IS NOT NULL 
                AND FileId <= 0;
            ");

            // Set remaining FileId references to NULL as a safety measure
            migrationBuilder.Sql(@"
                UPDATE EducationalContents 
                SET FileId = NULL 
                WHERE FileId IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
