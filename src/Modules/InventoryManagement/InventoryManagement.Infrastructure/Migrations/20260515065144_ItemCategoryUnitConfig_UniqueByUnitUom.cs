using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemCategoryUnitConfig_UniqueByUnitUom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemCategoryUnitConfig_ItemCategoryId_UnitId",
                schema: "Inventory",
                table: "ItemCategoryUnitConfig");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoryUnitConfig_ItemCategoryId_UnitId_UOMId",
                schema: "Inventory",
                table: "ItemCategoryUnitConfig",
                columns: new[] { "ItemCategoryId", "UnitId", "UOMId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemCategoryUnitConfig_ItemCategoryId_UnitId_UOMId",
                schema: "Inventory",
                table: "ItemCategoryUnitConfig");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoryUnitConfig_ItemCategoryId_UnitId",
                schema: "Inventory",
                table: "ItemCategoryUnitConfig",
                columns: new[] { "ItemCategoryId", "UnitId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
