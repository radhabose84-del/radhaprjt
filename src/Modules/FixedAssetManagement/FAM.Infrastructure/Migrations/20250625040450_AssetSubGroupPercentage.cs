using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetSubGroupPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdditionalDepreciation",
                schema: "FixedAsset",
                table: "AssetSubGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SubGroupPercentage",
                schema: "FixedAsset",
                table: "AssetSubGroup",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GroupPercentage",
                schema: "FixedAsset",
                table: "AssetGroup",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalDepreciation",
                schema: "FixedAsset",
                table: "AssetSubGroup");

            migrationBuilder.DropColumn(
                name: "SubGroupPercentage",
                schema: "FixedAsset",
                table: "AssetSubGroup");

            migrationBuilder.DropColumn(
                name: "GroupPercentage",
                schema: "FixedAsset",
                table: "AssetGroup");
        }
    }
}
