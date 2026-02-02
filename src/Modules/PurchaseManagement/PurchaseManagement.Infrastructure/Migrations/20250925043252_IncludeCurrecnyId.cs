using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncludeCurrecnyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Purchase",
                table: "QuotationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "SourceDetailId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Purchase",
                table: "QuotationDetail");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.AlterColumn<int>(
                name: "SourceDetailId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
