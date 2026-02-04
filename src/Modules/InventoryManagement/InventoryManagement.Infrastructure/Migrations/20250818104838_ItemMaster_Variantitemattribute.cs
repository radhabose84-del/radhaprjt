using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_Variantitemattribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeGroupId",
                principalSchema: "Inventory",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeGroupId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
