using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemSaleAddValuationMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ValuationMethodId",
                schema: "Inventory",
                table: "ItemSale",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSale_ValuationMethodId",
                schema: "Inventory",
                table: "ItemSale",
                column: "ValuationMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemSale_MiscMaster_ValuationMethodId",
                schema: "Inventory",
                table: "ItemSale",
                column: "ValuationMethodId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemSale_MiscMaster_ValuationMethodId",
                schema: "Inventory",
                table: "ItemSale");

            migrationBuilder.DropIndex(
                name: "IX_ItemSale_ValuationMethodId",
                schema: "Inventory",
                table: "ItemSale");

            migrationBuilder.DropColumn(
                name: "ValuationMethodId",
                schema: "Inventory",
                table: "ItemSale");
        }
    }
}
