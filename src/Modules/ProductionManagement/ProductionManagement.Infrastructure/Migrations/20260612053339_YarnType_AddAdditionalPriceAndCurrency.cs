using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class YarnType_AddAdditionalPriceAndCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalPrice",
                schema: "Production",
                table: "YarnType",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Production",
                table: "YarnType",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YarnType_CurrencyId",
                schema: "Production",
                table: "YarnType",
                column: "CurrencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YarnType_CurrencyId",
                schema: "Production",
                table: "YarnType");

            migrationBuilder.DropColumn(
                name: "AdditionalPrice",
                schema: "Production",
                table: "YarnType");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Production",
                table: "YarnType");
        }
    }
}
