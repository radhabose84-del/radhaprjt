using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Includestatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_MiscMasterId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Purchase",
                table: "PriceMasterDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "SourceFromId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Purchase",
                table: "PriceMasterDetail");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_MiscMasterId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "MiscMasterId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "SourceFromId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
