using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Item_Var : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_ItemId_Order",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "VariantAttributeId", "OptionValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_ItemId_Order",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                columns: new[] { "ItemId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_ItemId_Order",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "VariantAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_ItemId_Order",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                columns: new[] { "ItemId", "Order" });
        }
    }
}
