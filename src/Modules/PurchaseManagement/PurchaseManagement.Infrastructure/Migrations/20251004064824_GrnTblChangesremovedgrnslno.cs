using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GrnTblChangesremovedgrnslno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrnSlNo",
                schema: "Purchase",
                table: "GrnDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrnSlNo",
                schema: "Purchase",
                table: "GrnDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
