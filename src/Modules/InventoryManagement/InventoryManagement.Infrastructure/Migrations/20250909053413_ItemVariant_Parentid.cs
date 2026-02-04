using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemVariant_Parentid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ParentItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "ParentItemId");

            migrationBuilder.CreateIndex(
                name: "UX_ItemVariantValue_Item_VarAttr",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "VariantAttributeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_ParentItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "ParentItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_ParentItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ParentItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "UX_ItemVariantValue_Item_VarAttr",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropColumn(
                name: "ParentItemId",
                schema: "Inventory",
                table: "ItemVariantValue");
        }
    }
}
