using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Capitalpo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostCenterId",
                schema: "Purchase",
                table: "PurchaseLocalHeader");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "Purchase",
                table: "PurchaseLocalHeader");

            migrationBuilder.AddColumn<int>(
                name: "CapitalTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeader_CapitalTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                column: "CapitalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeader_PurchaseTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                column: "PurchaseTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderHeader_MiscMaster_CapitalTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                column: "CapitalTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderHeader_MiscMaster_PurchaseTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                column: "PurchaseTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderHeader_MiscMaster_CapitalTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderHeader_MiscMaster_PurchaseTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderHeader_CapitalTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderHeader_PurchaseTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "CapitalTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "PurchaseTypeId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                schema: "Purchase",
                table: "PurchaseLocalHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                schema: "Purchase",
                table: "PurchaseLocalHeader",
                type: "int",
                nullable: true);
        }
    }
}
