using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressesAndRenamePOImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AttachmentPath",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                newName: "POImage");

            migrationBuilder.AddColumn<string>(
                name: "BillingAddress",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingAddress",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.RenameColumn(
                name: "POImage",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                newName: "AttachmentPath");
        }
    }
}
