using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSlabSnapshotToSalesOrderDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountSlabId",
                schema: "Sales",
                table: "SalesOrderDiscount",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                schema: "Sales",
                table: "SalesOrderDiscount",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDiscount_DiscountSlabId",
                schema: "Sales",
                table: "SalesOrderDiscount",
                column: "DiscountSlabId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDiscount_DiscountSlab_DiscountSlabId",
                schema: "Sales",
                table: "SalesOrderDiscount",
                column: "DiscountSlabId",
                principalSchema: "Sales",
                principalTable: "DiscountSlab",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDiscount_DiscountSlab_DiscountSlabId",
                schema: "Sales",
                table: "SalesOrderDiscount");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDiscount_DiscountSlabId",
                schema: "Sales",
                table: "SalesOrderDiscount");

            migrationBuilder.DropColumn(
                name: "DiscountSlabId",
                schema: "Sales",
                table: "SalesOrderDiscount");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                schema: "Sales",
                table: "SalesOrderDiscount");
        }
    }
}
