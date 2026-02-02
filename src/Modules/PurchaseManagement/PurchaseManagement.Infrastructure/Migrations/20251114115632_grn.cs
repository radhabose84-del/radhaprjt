using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class grn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GSTPercentage",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.RenameColumn(
                name: "IsAttachmentPath",
                schema: "Purchase",
                table: "PurchaseBillEntryHeader",
                newName: "AttachmentPath");

            migrationBuilder.AddColumn<decimal>(
                name: "CSGTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountTotal",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ISGTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemsTotal",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MiscCharges",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseValue",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoundOff",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SSGTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxableAmount",
                schema: "Purchase",
                table: "GrnHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CGST",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GSTPercentage",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IGST",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemValue",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SGST",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxableAmount",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UOMId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CSGTTotal",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "DiscountTotal",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "ISGTTotal",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "ItemsTotal",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "MiscCharges",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "PurchaseValue",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "RoundOff",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "SSGTTotal",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "TaxableAmount",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "CGST",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "GSTPercentage",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "IGST",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "ItemValue",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "SGST",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "TaxableAmount",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "UOMId",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.RenameColumn(
                name: "AttachmentPath",
                schema: "Purchase",
                table: "PurchaseBillEntryHeader",
                newName: "IsAttachmentPath");

            migrationBuilder.AddColumn<decimal>(
                name: "GSTPercentage",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
