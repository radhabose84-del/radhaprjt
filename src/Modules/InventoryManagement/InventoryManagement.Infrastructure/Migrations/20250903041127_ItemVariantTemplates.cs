using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemVariantTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_ItemMaster_ItemMasterId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_MiscAttributeId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscTypeMaster_MiscAttributeGroupId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_ItemMasterId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_MiscAttributeGroupId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_MiscAttributeId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "ItemMasterId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "MiscAttributeGroupId",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "MiscAttributeId",
                table: "ItemVariantAttribute");

            migrationBuilder.RenameTable(
                name: "ItemVariantAttribute",
                newName: "ItemVariantAttribute",
                newSchema: "Inventory");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_ItemId_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                columns: new[] { "ItemId", "AttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_ItemId_Order",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                columns: new[] { "ItemId", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeGroupId",
                principalSchema: "Inventory",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_ItemId_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_ItemId_Order",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.RenameTable(
                name: "ItemVariantAttribute",
                schema: "Inventory",
                newName: "ItemVariantAttribute");

            migrationBuilder.AddColumn<int>(
                name: "ItemMasterId",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MiscAttributeGroupId",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscAttributeId",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_ItemMasterId",
                table: "ItemVariantAttribute",
                column: "ItemMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_MiscAttributeGroupId",
                table: "ItemVariantAttribute",
                column: "MiscAttributeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_MiscAttributeId",
                table: "ItemVariantAttribute",
                column: "MiscAttributeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_ItemMaster_ItemMasterId",
                table: "ItemVariantAttribute",
                column: "ItemMasterId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_MiscAttributeId",
                table: "ItemVariantAttribute",
                column: "MiscAttributeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscTypeMaster_MiscAttributeGroupId",
                table: "ItemVariantAttribute",
                column: "MiscAttributeGroupId",
                principalSchema: "Inventory",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id");
        }
    }
}
