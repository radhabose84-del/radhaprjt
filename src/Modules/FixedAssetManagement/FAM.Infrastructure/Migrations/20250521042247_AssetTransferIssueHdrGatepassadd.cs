using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetTransferIssueHdrGatepassadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GatePassNo",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr",
                type: "nvarchar(100)",
                nullable: true);

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GatePassNo",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr");

            
        }
    }
}
