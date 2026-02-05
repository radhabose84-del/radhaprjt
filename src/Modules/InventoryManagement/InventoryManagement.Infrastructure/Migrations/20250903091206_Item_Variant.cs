using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Item_Variant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_NewItemId",
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

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId_AttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropColumn(
                name: "AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropColumn(
                name: "AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropColumn(
                name: "NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.RenameColumn(
                name: "VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantValue",
                newName: "VariantAttributeId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemVariantValue_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantValue",
                newName: "IX_ItemVariantValue_VariantAttributeId");

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "VariantAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "VariantBasedOn");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "MiscMasterId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "VariantBasedOn",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_ItemVariantAttribute_VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "VariantAttributeId",
                principalSchema: "Inventory",
                principalTable: "ItemVariantAttribute",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_ItemVariantAttribute_VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.RenameColumn(
                name: "VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                newName: "VariantBasedOn");

            migrationBuilder.RenameIndex(
                name: "IX_ItemVariantValue_VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                newName: "IX_ItemVariantValue_VariantBasedOn");

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AddColumn<int>(
                name: "AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId_AttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "AttributeId", "OptionValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "NewItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "NewItemId",
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
    }
}
