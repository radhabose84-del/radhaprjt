using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WDVAssetSubGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AssetSubGroupId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "AssetSubGroupId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
