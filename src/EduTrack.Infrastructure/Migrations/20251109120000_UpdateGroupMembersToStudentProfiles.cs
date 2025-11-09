using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateGroupMembersToStudentProfiles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_GroupMembers_AspNetUsers_StudentId",
            table: "GroupMembers");

        migrationBuilder.DropIndex(
            name: "IX_GroupMembers_StudentId",
            table: "GroupMembers");

        migrationBuilder.DropIndex(
            name: "IX_GroupMembers_StudentGroupId_StudentId",
            table: "GroupMembers");

        migrationBuilder.AddColumn<int>(
            name: "StudentProfileId",
            table: "GroupMembers",
            type: "int",
            nullable: true);

        migrationBuilder.Sql("""
            INSERT INTO StudentProfiles (UserId, DisplayName, AvatarUrl, DateOfBirth, GradeLevel, Notes, IsArchived, CreatedAt, UpdatedAt)
            SELECT DISTINCT
                u.Id,
                LTRIM(RTRIM(CONCAT(
                    COALESCE(NULLIF(u.FirstName, ''), ''),
                    CASE WHEN NULLIF(u.FirstName, '') IS NULL OR NULLIF(u.LastName, '') IS NULL THEN '' ELSE ' ' END,
                    COALESCE(NULLIF(u.LastName, ''), u.UserName, 'Student')
                ))),
                NULL,
                NULL,
                NULL,
                NULL,
                0,
                SYSUTCDATETIME(),
                SYSUTCDATETIME()
            FROM AspNetUsers u
            INNER JOIN GroupMembers gm ON gm.StudentId = u.Id
            LEFT JOIN StudentProfiles sp ON sp.UserId = u.Id
            WHERE sp.Id IS NULL;
            """);

        migrationBuilder.Sql("""
            UPDATE gm
            SET StudentProfileId = sp.Id
            FROM GroupMembers gm
            INNER JOIN StudentProfiles sp ON sp.UserId = gm.StudentId;
            """);

        migrationBuilder.Sql("""
            UPDATE ce
            SET StudentProfileId = sp.Id
            FROM CourseEnrollments ce
            INNER JOIN StudentProfiles sp ON sp.UserId = ce.StudentId
            WHERE ce.StudentProfileId IS NULL;
            """);

        migrationBuilder.AlterColumn<int>(
            name: "StudentProfileId",
            table: "GroupMembers",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_GroupMembers_StudentProfileId",
            table: "GroupMembers",
            column: "StudentProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_GroupMembers_StudentGroupId_StudentProfileId",
            table: "GroupMembers",
            columns: new[] { "StudentGroupId", "StudentProfileId" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_GroupMembers_StudentProfiles_StudentProfileId",
            table: "GroupMembers",
            column: "StudentProfileId",
            principalTable: "StudentProfiles",
            principalColumn: "Id",
            onDelete: ReferentialAction.NoAction);

        migrationBuilder.DropForeignKey(
            name: "FK_ScheduleItemStudentAssignment_AspNetUsers_StudentId",
            table: "ScheduleItemStudentAssignment");

        migrationBuilder.DropIndex(
            name: "IX_ScheduleItemStudentAssignment_StudentId",
            table: "ScheduleItemStudentAssignment");

        migrationBuilder.DropIndex(
            name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId",
            table: "ScheduleItemStudentAssignment");

        migrationBuilder.DropIndex(
            name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentId_StudentProfileId",
            table: "ScheduleItemStudentAssignment");

        migrationBuilder.Sql("""
            INSERT INTO StudentProfiles (UserId, DisplayName, AvatarUrl, DateOfBirth, GradeLevel, Notes, IsArchived, CreatedAt, UpdatedAt)
            SELECT DISTINCT
                u.Id,
                LTRIM(RTRIM(CONCAT(
                    COALESCE(NULLIF(u.FirstName, ''), ''),
                    CASE WHEN NULLIF(u.FirstName, '') IS NULL OR NULLIF(u.LastName, '') IS NULL THEN '' ELSE ' ' END,
                    COALESCE(NULLIF(u.LastName, ''), u.UserName, 'Student')
                ))),
                NULL,
                NULL,
                NULL,
                NULL,
                0,
                SYSUTCDATETIME(),
                SYSUTCDATETIME()
            FROM AspNetUsers u
            INNER JOIN ScheduleItemStudentAssignment sia ON sia.StudentId = u.Id
            LEFT JOIN StudentProfiles sp ON sp.UserId = u.Id
            WHERE sp.Id IS NULL;
            """);

        migrationBuilder.Sql("""
            UPDATE sia
            SET StudentProfileId = sp.Id
            FROM ScheduleItemStudentAssignment sia
            INNER JOIN StudentProfiles sp ON sp.UserId = sia.StudentId
            WHERE sia.StudentProfileId IS NULL;
            """);

        migrationBuilder.AlterColumn<int>(
            name: "StudentProfileId",
            table: "ScheduleItemStudentAssignment",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ScheduleItemStudentAssignment_ScheduleItemId_StudentProfileId",
            table: "ScheduleItemStudentAssignment",
            columns: new[] { "ScheduleItemId", "StudentProfileId" },
            unique: true);

        migrationBuilder.DropColumn(
            name: "StudentId",
            table: "ScheduleItemStudentAssignment");

        migrationBuilder.DropColumn(
            name: "StudentId",
            table: "GroupMembers");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "StudentId",
            table: "GroupMembers",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql("""
            UPDATE gm
            SET StudentId = sp.UserId
            FROM GroupMembers gm
            INNER JOIN StudentProfiles sp ON sp.Id = gm.StudentProfileId;
            """);

        migrationBuilder.DropForeignKey(
            name: "FK_GroupMembers_StudentProfiles_StudentProfileId",
            table: "GroupMembers");

        migrationBuilder.DropIndex(
            name: "IX_GroupMembers_StudentProfileId",
            table: "GroupMembers");

        migrationBuilder.DropIndex(
            name: "IX_GroupMembers_StudentGroupId_StudentProfileId",
            table: "GroupMembers");

        migrationBuilder.AlterColumn<int>(
            name: "StudentProfileId",
            table: "GroupMembers",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.Sql("""
            UPDATE CourseEnrollments
            SET StudentProfileId = NULL;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_GroupMembers_StudentId",
            table: "GroupMembers",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_GroupMembers_StudentGroupId_StudentId",
            table: "GroupMembers",
            columns: new[] { "StudentGroupId", "StudentId" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_GroupMembers_AspNetUsers_StudentId",
            table: "GroupMembers",
            column: "StudentId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.NoAction);

        migrationBuilder.DropColumn(
            name: "StudentProfileId",
            table: "GroupMembers");
    }
}

