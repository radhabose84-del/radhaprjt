using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnTypeToNVARCHAR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                    migrationBuilder.AlterColumn<string>(
                name: "CustodianId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "NVARCHAR(50)",
                nullable: false);
                
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "NVARCHAR(50)",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                    migrationBuilder.AlterColumn<int>(
                name: "CustodianId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "int",
                nullable: false);
                
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "int",
                nullable: false);

          
        }
    }
}
