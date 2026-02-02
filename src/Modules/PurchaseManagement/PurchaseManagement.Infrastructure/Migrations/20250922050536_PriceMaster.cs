using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PriceMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_SourceFrom",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_SourceFrom",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropColumn(
                name: "UomId",
                schema: "Purchase",
                table: "PriceMasterDetail");

            migrationBuilder.RenameColumn(
                name: "SourceFrom",
                schema: "Purchase",
                table: "PriceMasterHeader",
                newName: "UomId");

            migrationBuilder.AddColumn<int>(
                name: "SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "SourceFromId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropColumn(
                name: "SourceFromId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.RenameColumn(
                name: "UomId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                newName: "SourceFrom");

            migrationBuilder.AddColumn<int>(
                name: "UomId",
                schema: "Purchase",
                table: "PriceMasterDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_SourceFrom",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "SourceFrom");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_SourceFrom",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "SourceFrom",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
