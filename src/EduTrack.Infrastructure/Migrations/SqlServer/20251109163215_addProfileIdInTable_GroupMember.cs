using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class addProfileIdInTable_GroupMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_AspNetUsers_StudentId",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_StudentGroupId_StudentId",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_StudentId",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "GroupMembers");

            migrationBuilder.AddColumn<int>(
                name: "StudentProfileId",
                table: "GroupMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_StudentGroupId_StudentProfileId",
                table: "GroupMembers",
                columns: new[] { "StudentGroupId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_StudentProfileId",
                table: "GroupMembers",
                column: "StudentProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_StudentProfiles_StudentProfileId",
                table: "GroupMembers",
                column: "StudentProfileId",
                principalTable: "StudentProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_StudentProfiles_StudentProfileId",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_StudentGroupId_StudentProfileId",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_StudentProfileId",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "StudentProfileId",
                table: "GroupMembers");

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "GroupMembers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_StudentGroupId_StudentId",
                table: "GroupMembers",
                columns: new[] { "StudentGroupId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_StudentId",
                table: "GroupMembers",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_AspNetUsers_StudentId",
                table: "GroupMembers",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
