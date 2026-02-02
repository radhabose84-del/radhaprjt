using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addcostcenterfrightmodecharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SLATerms",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                newName: "TermDescription");

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightCharges",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermsId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceHeader_ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                column: "ModeOfDispatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderServiceHeader_MiscMaster_ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                column: "ModeOfDispatchId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderServiceHeader_MiscMaster_ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderServiceHeader_ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.DropColumn(
                name: "FreightCharges",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.DropColumn(
                name: "ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.DropColumn(
                name: "TermsId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader");

            migrationBuilder.RenameColumn(
                name: "TermDescription",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                newName: "SLATerms");
        }
    }
}
