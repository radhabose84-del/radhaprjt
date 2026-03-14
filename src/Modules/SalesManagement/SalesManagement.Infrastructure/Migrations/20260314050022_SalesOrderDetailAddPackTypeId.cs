using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderDetailAddPackTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackTypeId",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_PackTypeId",
                schema: "Sales",
                table: "SalesOrderDetail",
                column: "PackTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderDetail_PackType_PackTypeId",
                schema: "Sales",
                table: "SalesOrderDetail",
                column: "PackTypeId",
                principalSchema: "Sales",
                principalTable: "PackType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderDetail_PackType_PackTypeId",
                schema: "Sales",
                table: "SalesOrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDetail_PackTypeId",
                schema: "Sales",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "PackTypeId",
                schema: "Sales",
                table: "SalesOrderDetail");
        }
    }
}
