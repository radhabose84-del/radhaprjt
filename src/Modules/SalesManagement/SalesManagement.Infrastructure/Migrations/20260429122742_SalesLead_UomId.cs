using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesLead_UomId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UomId",
                schema: "Sales",
                table: "SalesLead",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_UomId",
                schema: "Sales",
                table: "SalesLead",
                column: "UomId",
                filter: "[UomId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesLead_UomId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropColumn(
                name: "UomId",
                schema: "Sales",
                table: "SalesLead");
        }
    }
}
