using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_AddPriceGroupId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceGroupId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_PriceGroupId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "PriceGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_PriceGroupMaster_PriceGroupId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "PriceGroupId",
                principalSchema: "Inventory",
                principalTable: "PriceGroupMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemMaster_PriceGroupMaster_PriceGroupId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropIndex(
                name: "IX_ItemMaster_PriceGroupId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "PriceGroupId",
                schema: "Inventory",
                table: "ItemMaster");
        }
    }
}
