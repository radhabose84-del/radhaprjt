using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Item_Hsn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HsnId",
                schema: "Purchase",
                table: "RfqItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HsnId",
                schema: "Purchase",
                table: "QuotationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HsnId",
                schema: "Purchase",
                table: "RfqItem");

            migrationBuilder.DropColumn(
                name: "HsnId",
                schema: "Purchase",
                table: "QuotationDetail");
        }
    }
}
