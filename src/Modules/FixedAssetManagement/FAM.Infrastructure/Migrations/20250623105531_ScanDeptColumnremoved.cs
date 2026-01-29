using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScanDeptColumnremoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScanDepartment",
                schema: "FixedAsset",
                table: "AssetAudit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScanDepartment",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(200)",
                nullable: true);
        }
    }
}
