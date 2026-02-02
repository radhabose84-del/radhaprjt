using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PINewColumnInclude : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemCategoryId",
                schema: "Purchase",
                table: "IndentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItemUOMId",
                schema: "Purchase",
                table: "IndentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                schema: "Purchase",
                table: "IndentDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemCategoryId",
                schema: "Purchase",
                table: "IndentDetail");

            migrationBuilder.DropColumn(
                name: "ItemUOMId",
                schema: "Purchase",
                table: "IndentDetail");

            migrationBuilder.DropColumn(
                name: "Rate",
                schema: "Purchase",
                table: "IndentDetail");
        }
    }
}
