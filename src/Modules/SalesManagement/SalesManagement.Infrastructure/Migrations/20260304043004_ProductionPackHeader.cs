using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductionPackHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackHeader_MiscMaster_StatusId",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackHeader_StatusId",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Sales",
                table: "StockLedger",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Sales",
                table: "StockLedger");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Production",
                table: "ProductionPackHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_StatusId",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackHeader_MiscMaster_StatusId",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "StatusId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
