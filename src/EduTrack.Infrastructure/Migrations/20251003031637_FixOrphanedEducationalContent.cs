using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrphanedEducationalContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix orphaned EducationalContent records that reference non-existent Files
            migrationBuilder.Sql(@"
                UPDATE EducationalContents 
                SET FileId = NULL 
                WHERE FileId IS NOT NULL 
                AND FileId NOT IN (SELECT Id FROM Files);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
