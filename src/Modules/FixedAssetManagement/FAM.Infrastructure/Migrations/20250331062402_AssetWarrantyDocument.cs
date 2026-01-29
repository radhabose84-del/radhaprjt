using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetWarrantyDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
            name: "Document",
            schema: "FixedAsset",
            table: "AssetWarranty",
            type: "nvarchar(255)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
            name: "Document",
            schema: "FixedAsset",
            table: "AssetWarranty",
            type: "nvarchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldNullable: true);
        }
    }
}
