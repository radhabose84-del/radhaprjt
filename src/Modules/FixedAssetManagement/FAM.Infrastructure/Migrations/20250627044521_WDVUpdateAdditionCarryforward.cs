using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WDVUpdateAdditionCarryforward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastYearAdditionalDepreciation",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LastYearAdditionalDepreciation",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
