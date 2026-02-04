using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_Variantitemgroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeGroupId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropColumn(
                name: "AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");
        }
    }
}
