using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_deletion_behaviour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_MiscMaster_DefaultMaterialRequestTypeId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_MiscMaster_RequestTypeId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_MiscMaster_ValuationMethodId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemManufacture_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemManufacture");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemManufacture_MiscMaster_ManufacturingTypeId",
                schema: "Inventory",
                table: "ItemManufacture");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemPurchase_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemQuality_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemQuality");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemSupplier_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemUOM_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemUOM");

            migrationBuilder.DropIndex(
                name: "IX_ItemUOM_ItemId",
                schema: "Inventory",
                table: "ItemUOM");

            migrationBuilder.DropIndex(
                name: "IX_ItemSupplier_ItemId",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropIndex(
                name: "IX_ItemManufacture_ItemId",
                schema: "Inventory",
                table: "ItemManufacture");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUOM_ItemId",
                schema: "Inventory",
                table: "ItemUOM",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSupplier_ItemId",
                schema: "Inventory",
                table: "ItemSupplier",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemManufacture_ItemId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_MiscMaster_DefaultMaterialRequestTypeId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "DefaultMaterialRequestTypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_MiscMaster_RequestTypeId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "RequestTypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_MiscMaster_ValuationMethodId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "ValuationMethodId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemManufacture_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemManufacture_MiscMaster_ManufacturingTypeId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ManufacturingTypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemPurchase_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemPurchase",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemQuality_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemQuality",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemSupplier_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemSupplier",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemUOM_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemUOM",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_MiscMaster_DefaultMaterialRequestTypeId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_MiscMaster_RequestTypeId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_MiscMaster_ValuationMethodId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemManufacture_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemManufacture");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemManufacture_MiscMaster_ManufacturingTypeId",
                schema: "Inventory",
                table: "ItemManufacture");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemPurchase_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemQuality_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemQuality");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemSupplier_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemUOM_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemUOM");

            migrationBuilder.DropIndex(
                name: "IX_ItemUOM_ItemId",
                schema: "Inventory",
                table: "ItemUOM");

            migrationBuilder.DropIndex(
                name: "IX_ItemSupplier_ItemId",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropIndex(
                name: "IX_ItemManufacture_ItemId",
                schema: "Inventory",
                table: "ItemManufacture");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUOM_ItemId",
                schema: "Inventory",
                table: "ItemUOM",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSupplier_ItemId",
                schema: "Inventory",
                table: "ItemSupplier",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemManufacture_ItemId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_MiscMaster_DefaultMaterialRequestTypeId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "DefaultMaterialRequestTypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_MiscMaster_RequestTypeId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "RequestTypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_MiscMaster_ValuationMethodId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "ValuationMethodId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemManufacture_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemManufacture_MiscMaster_ManufacturingTypeId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ManufacturingTypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemPurchase_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemPurchase",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemQuality_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemQuality",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemSupplier_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemSupplier",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemUOM_ItemMaster_ItemId",
                schema: "Inventory",
                table: "ItemUOM",
                column: "ItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
