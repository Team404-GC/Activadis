using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activadis.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedActivityCategoriesAndMatchActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActivityId",
                table: "Matches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityCategories",
                columns: table => new
                {
                    ActivitiesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCategories", x => new { x.ActivitiesId, x.CategoriesId });
                    table.ForeignKey(
                        name: "FK_ActivityCategories_Activities_ActivitiesId",
                        column: x => x.ActivitiesId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityCategories_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ActivityId",
                table: "Matches",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCategories_CategoriesId",
                table: "ActivityCategories",
                column: "CategoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Activities_ActivityId",
                table: "Matches",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Activities_ActivityId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "ActivityCategories");

            migrationBuilder.DropIndex(
                name: "IX_Matches_ActivityId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Matches");
        }
    }
}
