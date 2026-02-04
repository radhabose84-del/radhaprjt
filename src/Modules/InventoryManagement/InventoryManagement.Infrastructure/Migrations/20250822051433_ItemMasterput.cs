using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMasterput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PutAwayRule_ItemMaster_ItemMasterId",
                schema: "Inventory",
                table: "PutAwayRule");

            migrationBuilder.DropIndex(
                name: "IX_PutAwayRule_ItemMasterId",
                schema: "Inventory",
                table: "PutAwayRule");

            migrationBuilder.DropColumn(
                name: "ItemMasterId",
                schema: "Inventory",
                table: "PutAwayRule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemMasterId",
                schema: "Inventory",
                table: "PutAwayRule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemMasterId",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_PutAwayRule_ItemMaster_ItemMasterId",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemMasterId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id");
        }
    }
}
