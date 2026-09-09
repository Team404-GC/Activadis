using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activadis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangedActivityAndSignUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SignUps_Activities_ActivityId",
                table: "SignUps");

            migrationBuilder.AddForeignKey(
                name: "FK_SignUps_Activities_ActivityId",
                table: "SignUps",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SignUps_Activities_ActivityId",
                table: "SignUps");

            migrationBuilder.AddForeignKey(
                name: "FK_SignUps_Activities_ActivityId",
                table: "SignUps",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id");
        }
    }
}
