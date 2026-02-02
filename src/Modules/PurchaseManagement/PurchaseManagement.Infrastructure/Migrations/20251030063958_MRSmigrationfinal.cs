using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MRSmigrationfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledQuantity",
                schema: "Purchase",
                table: "MrsDetail");

            migrationBuilder.DropColumn(
                name: "IssuedQuantity",
                schema: "Purchase",
                table: "MrsDetail");

            migrationBuilder.AddColumn<int>(
                name: "SubStoresWarehouseId",
                schema: "Purchase",
                table: "MrsHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseStockId",
                schema: "Purchase",
                table: "MrsDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubStoresWarehouseId",
                schema: "Purchase",
                table: "MrsHeader");

            migrationBuilder.DropColumn(
                name: "WarehouseStockId",
                schema: "Purchase",
                table: "MrsDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "CancelledQuantity",
                schema: "Purchase",
                table: "MrsDetail",
                type: "decimal(18,3)",
                nullable: true,
                defaultValue: 0.000m);

            migrationBuilder.AddColumn<decimal>(
                name: "IssuedQuantity",
                schema: "Purchase",
                table: "MrsDetail",
                type: "decimal(18,3)",
                nullable: true,
                defaultValue: 0.000m);
        }
    }
}
