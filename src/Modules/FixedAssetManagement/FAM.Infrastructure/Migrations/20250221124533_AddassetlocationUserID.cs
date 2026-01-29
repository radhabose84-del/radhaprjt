using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddassetlocationUserID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AddColumn<int>(
                name: "CustodianId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "int",
                nullable: false,
                defaultValue: 0);

          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "CustodianId",
                schema: "FixedAsset",
                table: "AssetLocation");

            migrationBuilder.DropColumn(
                name: "UserID",
                schema: "FixedAsset",
                table: "AssetLocation");

            
        }
    }
}
