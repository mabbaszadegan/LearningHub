using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupOrphanedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clean up orphaned EducationalContent records that reference non-existent Files
            migrationBuilder.Sql(@"
                UPDATE EducationalContents 
                SET FileId = NULL 
                WHERE FileId IS NOT NULL 
                AND FileId NOT IN (SELECT Id FROM Files);
            ");

            // Also clean up any EducationalContent records that have invalid FileId values
            migrationBuilder.Sql(@"
                UPDATE EducationalContents 
                SET FileId = NULL 
                WHERE FileId IS NOT NULL 
                AND FileId <= 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
