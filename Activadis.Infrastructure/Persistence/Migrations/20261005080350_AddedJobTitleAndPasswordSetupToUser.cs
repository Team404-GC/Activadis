using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activadis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedJobTitleAndPasswordSetupToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordSetupTokenExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordSetupTokenHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSetupTokenExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSetupTokenHash",
                table: "Users");
        }
    }
}
