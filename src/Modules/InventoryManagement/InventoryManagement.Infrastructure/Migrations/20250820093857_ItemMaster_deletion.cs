using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_deletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemMaster_MiscMaster_ItemClassificationId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemMaster_MiscMaster_XPlantMaterialStatusId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_MiscMaster_ItemClassificationId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "ItemClassificationId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_MiscMaster_XPlantMaterialStatusId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "XPlantMaterialStatusId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "VariantBasedOn",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
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
                name: "FK_ItemMaster_MiscMaster_ItemClassificationId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemMaster_MiscMaster_XPlantMaterialStatusId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_MiscMaster_ItemClassificationId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "ItemClassificationId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_MiscMaster_XPlantMaterialStatusId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "XPlantMaterialStatusId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "VariantBasedOn",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeGroupId",
                principalSchema: "Inventory",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id");
        }
    }
}
