using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BilEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorId",
                schema: "Purchase",
                table: "PurchaseBillEntryHeader",
                newName: "PartyId");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseBillEntryHeader",
                newName: "PoId");

            migrationBuilder.AddColumn<decimal>(
                name: "PoDetailId",
                schema: "Purchase",
                table: "PurchaseBillEntryDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PoDetailId",
                schema: "Purchase",
                table: "PurchaseBillEntryDetail");

            migrationBuilder.RenameColumn(
                name: "PoId",
                schema: "Purchase",
                table: "PurchaseBillEntryHeader",
                newName: "PurchaseOrderId");

            migrationBuilder.RenameColumn(
                name: "PartyId",
                schema: "Purchase",
                table: "PurchaseBillEntryHeader",
                newName: "VendorId");
        }
    }
}
