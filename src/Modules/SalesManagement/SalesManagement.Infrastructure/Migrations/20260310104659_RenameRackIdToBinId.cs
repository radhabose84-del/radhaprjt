using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameRackIdToBinId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RackId",
                schema: "Sales",
                table: "StoReceiptHeader",
                newName: "BinId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BinId",
                schema: "Sales",
                table: "StoReceiptHeader",
                newName: "RackId");
        }
    }
}
