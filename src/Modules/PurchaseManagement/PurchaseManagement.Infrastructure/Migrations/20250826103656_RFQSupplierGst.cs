using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RFQSupplierGst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gst",
                schema: "Purchase",
                table: "RfqSuppliers");

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "Purchase",
                table: "RfqSuppliers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GSTNumber",
                schema: "Purchase",
                table: "RfqSuppliers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GSTNumber",
                schema: "Purchase",
                table: "RfqSuppliers");

            migrationBuilder.AlterColumn<string>(
                name: "Mobile",
                schema: "Purchase",
                table: "RfqSuppliers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gst",
                schema: "Purchase",
                table: "RfqSuppliers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
