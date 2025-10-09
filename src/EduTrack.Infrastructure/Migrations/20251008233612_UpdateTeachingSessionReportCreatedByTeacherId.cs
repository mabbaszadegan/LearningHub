﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeachingSessionReportCreatedByTeacherId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatedByTeacherId",
                table: "TeachingSessionReports",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CreatedByTeacherId",
                table: "TeachingSessionReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }
    }
}
