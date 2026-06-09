using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesType_AddSalesGroupId_SalesOfficeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesGroupId",
                schema: "Party",
                table: "SalesType",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesOfficeId",
                schema: "Party",
                table: "SalesType",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesType_SalesGroupId",
                schema: "Party",
                table: "SalesType",
                column: "SalesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesType_SalesOfficeId",
                schema: "Party",
                table: "SalesType",
                column: "SalesOfficeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesType_SalesGroupId",
                schema: "Party",
                table: "SalesType");

            migrationBuilder.DropIndex(
                name: "IX_SalesType_SalesOfficeId",
                schema: "Party",
                table: "SalesType");

            migrationBuilder.DropColumn(
                name: "SalesGroupId",
                schema: "Party",
                table: "SalesType");

            migrationBuilder.DropColumn(
                name: "SalesOfficeId",
                schema: "Party",
                table: "SalesType");
        }
    }
}
