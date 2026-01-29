using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WDVUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalCarryForward",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LastYearAdditionalDepreciation",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalCarryForward",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail");

            migrationBuilder.DropColumn(
                name: "LastYearAdditionalDepreciation",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
