using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusIdToSalesOrderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_StatusId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_StatusId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "StatusId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_StatusId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_StatusId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
