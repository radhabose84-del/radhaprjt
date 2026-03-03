using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderHeader_DispatchLocationTypeForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_DispatchLocationType",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "DispatchLocationType");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_DispatchLocationType",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "DispatchLocationType",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_DispatchLocationType",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_DispatchLocationType",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
