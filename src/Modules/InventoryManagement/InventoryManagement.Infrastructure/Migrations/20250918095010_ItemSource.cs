using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "SourceOfItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_MiscMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "SourceOfItemId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemMaster_MiscMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropIndex(
                name: "IX_ItemMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster");
        }
    }
}
