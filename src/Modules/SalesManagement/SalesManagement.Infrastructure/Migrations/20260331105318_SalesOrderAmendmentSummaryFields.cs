using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderAmendmentSummaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GSTPercentage",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TCSPercentage",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxableAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalBags",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscountPerKg",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFreight",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGST",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalTCS",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeightKgs",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWithGST",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetRatePerKg",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PendingQty",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TCSAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxableAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "GSTPercentage",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "ItemValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TCSPercentage",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TaxableAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalBags",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalDiscountPerKg",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalFreight",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalGST",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalTCS",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalWeightKgs",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalWithGST",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");

            migrationBuilder.DropColumn(
                name: "NetRatePerKg",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");

            migrationBuilder.DropColumn(
                name: "PendingQty",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");

            migrationBuilder.DropColumn(
                name: "TCSAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");

            migrationBuilder.DropColumn(
                name: "TaxableAmount",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");
        }
    }
}
