using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDiscountPlanAndPaymentTermsFromSalesOrderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_DiscountPlanId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_DiscountPlanId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "DiscountPlanId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "PaymentTermsId",
                schema: "Sales",
                table: "SalesOrderHeader");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountPlanId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermsId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_DiscountPlanId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "DiscountPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_DiscountPlanId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "DiscountPlanId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
