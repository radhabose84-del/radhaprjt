using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitIdAndInvFlgToDispatchAdviceHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InvFlg",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvFlg",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Sales",
                table: "DispatchAdviceHeader");
        }
    }
}
