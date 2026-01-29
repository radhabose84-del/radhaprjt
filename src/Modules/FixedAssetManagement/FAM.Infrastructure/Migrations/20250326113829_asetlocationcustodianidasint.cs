using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class asetlocationcustodianidasint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(50)");

            migrationBuilder.AlterColumn<int>(
                name: "CustodianId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR(50)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "NVARCHAR(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CustodianId",
                schema: "FixedAsset",
                table: "AssetLocation",
                type: "NVARCHAR(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
