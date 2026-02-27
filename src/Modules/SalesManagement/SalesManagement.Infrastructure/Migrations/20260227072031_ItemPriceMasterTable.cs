using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemPriceMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SalesItemPriceMaster",
                schema: "Sales",
                newName: "ItemPriceMaster",
                newSchema: "Sales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ItemPriceMaster",
                schema: "Sales",
                newName: "SalesItemPriceMaster",
                newSchema: "Sales");
        }
    }
}
