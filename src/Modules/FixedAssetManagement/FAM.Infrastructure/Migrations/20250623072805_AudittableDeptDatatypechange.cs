using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AudittableDeptDatatypechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScanDepartmentId",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.AddColumn<string>(
                name: "ScanDepartment",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(200)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScanDepartment",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.AddColumn<int>(
                name: "ScanDepartmentId",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "int",
                nullable: true);
        }
    }
}
