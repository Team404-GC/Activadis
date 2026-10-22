using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activadis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedCommentToSignUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "SignUps",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "SignUps");
        }
    }
}
