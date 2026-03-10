using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionKgsLooseKgsNoOfBags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LooseKgs",
                schema: "Production",
                table: "ProductionPackHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionKgs",
                schema: "Production",
                table: "ProductionPackHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NoOfBags",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LooseKgs",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.DropColumn(
                name: "ProductionKgs",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.DropColumn(
                name: "NoOfBags",
                schema: "Production",
                table: "ProductionPackDetail");
        }
    }
}
