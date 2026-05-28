using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatusIdFromVendorEvaluationHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VendorEvaluationHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "VendorEvaluationHeader");

            migrationBuilder.DropIndex(
                name: "IX_VendorEvaluationHeader_StatusId",
                schema: "Purchase",
                table: "VendorEvaluationHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "VendorEvaluationHeader");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationHeader_StatusId",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_VendorEvaluationHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                column: "StatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
