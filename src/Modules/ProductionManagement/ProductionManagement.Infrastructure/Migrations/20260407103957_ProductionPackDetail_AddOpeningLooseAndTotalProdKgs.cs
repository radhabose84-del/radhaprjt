using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductionPackDetail_AddOpeningLooseAndTotalProdKgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OpeningLooseKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalProductionKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningLooseKgs",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "TotalProductionKgs",
                schema: "Production",
                table: "ProductionPackDetail");
        }
    }
}
