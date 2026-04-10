using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ItemSpecificationRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalValue",
                schema: "Sales",
                table: "ItemPriceMaster");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalValue",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);
        }
    }
}
