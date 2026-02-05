using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemVariantTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                schema: "Inventory",
                table: "ItemMaster",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)");

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                schema: "Inventory",
                table: "ItemMaster",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.CreateTable(
                name: "ItemVariantAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemMasterId = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    MiscAttributeId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    AttributeGroupId = table.Column<int>(type: "int", nullable: true),
                    MiscAttributeGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemVariantAttribute", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemVariantAttribute_ItemMaster_ItemMasterId",
                        column: x => x.ItemMasterId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemVariantAttribute_MiscMaster_MiscAttributeId",
                        column: x => x.MiscAttributeId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemVariantAttribute_MiscTypeMaster_MiscAttributeGroupId",
                        column: x => x.MiscAttributeGroupId,
                        principalSchema: "Inventory",
                        principalTable: "MiscTypeMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId_AttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "AttributeId", "OptionValue" },
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId_AttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.AlterColumn<string>(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                schema: "Inventory",
                table: "ItemMaster",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ItemCode",
                schema: "Inventory",
                table: "ItemMaster",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "ItemId");
        }
    }
}
