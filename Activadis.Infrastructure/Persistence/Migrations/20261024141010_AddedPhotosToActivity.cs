using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Activadis.Infrastructure.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class AddedPhotosToActivity : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "ActivityPhotos",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Image = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
					ImageContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
					SortOrder = table.Column<int>(type: "int", nullable: false),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
					DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
					ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ActivityPhotos", x => x.Id);
					table.ForeignKey(
						name: "FK_ActivityPhotos_Activities_ActivityId",
						column: x => x.ActivityId,
						principalTable: "Activities",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_ActivityPhotos_ActivityId_SortOrder",
				table: "ActivityPhotos",
				columns: new[] { "ActivityId", "SortOrder" });

			migrationBuilder.Sql(@"
                INSERT INTO [ActivityPhotos] ([Id], [ActivityId], [Image], [ImageContentType], [SortOrder], [CreatedAt])
                SELECT NEWID(), [Id], [Image], [ImageContentType], 0, [CreatedAt]
                FROM [Activities]
                WHERE DATALENGTH([Image]) > 0");

			migrationBuilder.DropColumn(
				name: "Image",
				table: "Activities");

			migrationBuilder.DropColumn(
				name: "ImageContentType",
				table: "Activities");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<byte[]>(
				name: "Image",
				table: "Activities",
				type: "varbinary(max)",
				nullable: false,
				defaultValue: new byte[0]);

			migrationBuilder.AddColumn<string>(
				name: "ImageContentType",
				table: "Activities",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "");

			migrationBuilder.Sql(@"
                UPDATE [a]
                SET [a].[Image] = [p].[Image], [a].[ImageContentType] = [p].[ImageContentType]
                FROM [Activities] AS [a]
                INNER JOIN [ActivityPhotos] AS [p] ON [p].[ActivityId] = [a].[Id]
                WHERE [p].[SortOrder] = 0 AND [p].[DeletedAt] IS NULL");

			migrationBuilder.DropTable(
				name: "ActivityPhotos");
		}
	}
}