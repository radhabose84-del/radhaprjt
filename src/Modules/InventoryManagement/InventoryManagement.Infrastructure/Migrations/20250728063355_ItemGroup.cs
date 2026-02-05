using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GroupName",
                schema: "Inventory",
                table: "ItemGroup",
                newName: "ItemGroupName");

            migrationBuilder.RenameColumn(
                name: "GroupCode",
                schema: "Inventory",
                table: "ItemGroup",
                newName: "ItemGroupCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ItemGroupName",
                schema: "Inventory",
                table: "ItemGroup",
                newName: "GroupName");

            migrationBuilder.RenameColumn(
                name: "ItemGroupCode",
                schema: "Inventory",
                table: "ItemGroup",
                newName: "GroupCode");
        }
    }
}
