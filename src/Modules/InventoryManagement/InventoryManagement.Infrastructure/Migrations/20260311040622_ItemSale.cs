using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemSale",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    MinQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    PackageQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true),
                    DeliveryLeadTime = table.Column<int>(type: "int", nullable: true),
                    Discount = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemSale_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemSale_UOM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UOM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemSale_ItemId",
                schema: "Inventory",
                table: "ItemSale",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSale_UomId",
                schema: "Inventory",
                table: "ItemSale",
                column: "UomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemSale",
                schema: "Inventory");
        }
    }
}
