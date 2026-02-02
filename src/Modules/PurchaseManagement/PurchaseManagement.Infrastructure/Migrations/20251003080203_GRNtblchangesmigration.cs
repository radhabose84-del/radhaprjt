using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GRNtblchangesmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UomId",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.AlterColumn<string>(
                name: "DcNo",
                schema: "Purchase",
                table: "GrnHeader",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DcNo",
                schema: "Purchase",
                table: "GrnHeader",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UomId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
