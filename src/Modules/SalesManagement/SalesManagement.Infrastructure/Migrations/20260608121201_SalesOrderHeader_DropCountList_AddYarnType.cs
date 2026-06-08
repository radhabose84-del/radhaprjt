using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderHeader_DropCountList_AddYarnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_CountListId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_CountListId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "CountListId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.AddColumn<int>(
                name: "YarnTypeId",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_YarnTypeId",
                schema: "Sales",
                table: "SalesOrderDetail",
                column: "YarnTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesOrderDetail_YarnTypeId",
                schema: "Sales",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "YarnTypeId",
                schema: "Sales",
                table: "SalesOrderDetail");

            migrationBuilder.AddColumn<int>(
                name: "CountListId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_CountListId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "CountListId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_CountListId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "CountListId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
