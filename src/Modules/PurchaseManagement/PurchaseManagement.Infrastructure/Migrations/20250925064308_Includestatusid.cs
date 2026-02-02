using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Includestatusid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApprove",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_StatusId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "StatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceMasterHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropIndex(
                name: "IX_PriceMasterHeader_StatusId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "PriceMasterHeader");

            migrationBuilder.AddColumn<byte>(
                name: "IsApprove",
                schema: "Purchase",
                table: "PriceMasterHeader",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
