using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activadis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedSlugsToUserActivityAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Activities");
        }
    }
}
