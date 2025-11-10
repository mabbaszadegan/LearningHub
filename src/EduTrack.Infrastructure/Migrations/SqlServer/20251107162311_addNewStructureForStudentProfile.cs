using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class addNewStructureForStudentProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('CourseEnrollments', 'StudentProfileId') IS NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_CourseEnrollments_CourseId_StudentId'
            AND object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        DROP INDEX [IX_CourseEnrollments_CourseId_StudentId] ON [CourseEnrollments];

    ALTER TABLE [CourseEnrollments] ADD [StudentProfileId] INT NULL;

    CREATE UNIQUE INDEX [IX_CourseEnrollments_CourseId_StudentId]
        ON [CourseEnrollments] ([CourseId], [StudentId])
        WHERE [StudentProfileId] IS NULL;

    CREATE UNIQUE INDEX [IX_CourseEnrollments_CourseId_StudentId_StudentProfileId]
        ON [CourseEnrollments] ([CourseId], [StudentId], [StudentProfileId])
        WHERE [StudentProfileId] IS NOT NULL;

    CREATE INDEX [IX_CourseEnrollments_StudentProfileId]
        ON [CourseEnrollments] ([StudentProfileId]);

    ALTER TABLE [CourseEnrollments]
        WITH CHECK ADD CONSTRAINT [FK_CourseEnrollments_StudentProfiles_StudentProfileId]
        FOREIGN KEY([StudentProfileId]) REFERENCES [StudentProfiles] ([Id]);
END
ELSE
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_CourseEnrollments_StudentProfiles_StudentProfileId'
            AND parent_object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        ALTER TABLE [CourseEnrollments]
            WITH CHECK ADD CONSTRAINT [FK_CourseEnrollments_StudentProfiles_StudentProfileId]
            FOREIGN KEY([StudentProfileId]) REFERENCES [StudentProfiles] ([Id]);

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_CourseEnrollments_CourseId_StudentId'
            AND object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        CREATE UNIQUE INDEX [IX_CourseEnrollments_CourseId_StudentId]
            ON [CourseEnrollments] ([CourseId], [StudentId])
            WHERE [StudentProfileId] IS NULL;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_CourseEnrollments_CourseId_StudentId_StudentProfileId'
            AND object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        CREATE UNIQUE INDEX [IX_CourseEnrollments_CourseId_StudentId_StudentProfileId]
            ON [CourseEnrollments] ([CourseId], [StudentId], [StudentProfileId])
            WHERE [StudentProfileId] IS NOT NULL;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_CourseEnrollments_StudentProfileId'
            AND object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        CREATE INDEX [IX_CourseEnrollments_StudentProfileId]
            ON [CourseEnrollments] ([StudentProfileId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('CourseEnrollments', 'StudentProfileId') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_CourseEnrollments_StudentProfiles_StudentProfileId'
            AND parent_object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        ALTER TABLE [CourseEnrollments]
            DROP CONSTRAINT [FK_CourseEnrollments_StudentProfiles_StudentProfileId];

    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_CourseEnrollments_CourseId_StudentId'
            AND object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        DROP INDEX [IX_CourseEnrollments_CourseId_StudentId] ON [CourseEnrollments];

    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_CourseEnrollments_CourseId_StudentId_StudentProfileId'
            AND object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        DROP INDEX [IX_CourseEnrollments_CourseId_StudentId_StudentProfileId] ON [CourseEnrollments];

    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_CourseEnrollments_StudentProfileId'
            AND object_id = OBJECT_ID(N'dbo.CourseEnrollments')
    )
        DROP INDEX [IX_CourseEnrollments_StudentProfileId] ON [CourseEnrollments];

    ALTER TABLE [CourseEnrollments] DROP COLUMN [StudentProfileId];

    CREATE UNIQUE INDEX [IX_CourseEnrollments_CourseId_StudentId]
        ON [CourseEnrollments] ([CourseId], [StudentId]);
END
");
        }
    }
}
