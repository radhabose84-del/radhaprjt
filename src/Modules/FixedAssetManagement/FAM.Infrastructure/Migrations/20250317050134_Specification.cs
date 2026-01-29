using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Specification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelNumber",
                schema: "FixedAsset",
                table: "AssetSpecifications");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                schema: "FixedAsset",
                table: "AssetSpecifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelNumber",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                type: "varchar(100)",
                nullable: true);
        }
    }
}
