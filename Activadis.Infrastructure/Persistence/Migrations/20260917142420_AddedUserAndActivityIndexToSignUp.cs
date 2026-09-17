using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activadis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserAndActivityIndexToSignUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SignUps_ActivityId",
                table: "SignUps");

            migrationBuilder.CreateIndex(
                name: "IX_SignUps_ActivityId_UserId",
                table: "SignUps",
                columns: new[] { "ActivityId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SignUps_ActivityId_UserId",
                table: "SignUps");

            migrationBuilder.CreateIndex(
                name: "IX_SignUps_ActivityId",
                table: "SignUps",
                column: "ActivityId");
        }
    }
}
