using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class poamendment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmendmentReason",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldPOId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisionNo",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmendmentReason",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "OldPOId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "RevisionNo",
                schema: "Purchase",
                table: "PurchaseOrderHeader");
        }
    }
}
