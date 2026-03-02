using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameExMillPriceToExMillRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExMillPrice",
                schema: "Sales",
                table: "ItemPriceMaster",
                newName: "ExMillRate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExMillRate",
                schema: "Sales",
                table: "ItemPriceMaster",
                newName: "ExMillPrice");
        }
    }
}
