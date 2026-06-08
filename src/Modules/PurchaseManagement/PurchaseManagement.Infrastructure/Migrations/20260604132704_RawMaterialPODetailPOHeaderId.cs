using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RawMaterialPODetailPOHeaderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawMaterialPODetail_RawMaterialPOHeader_RawMaterialPOId",
                schema: "Purchase",
                table: "RawMaterialPODetail");

            migrationBuilder.RenameColumn(
                name: "RawMaterialPOId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                newName: "POHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_RawMaterialPODetail_RawMaterialPOId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                newName: "IX_RawMaterialPODetail_POHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_RawMaterialPODetail_RawMaterialPOHeader_POHeaderId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                column: "POHeaderId",
                principalSchema: "Purchase",
                principalTable: "RawMaterialPOHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawMaterialPODetail_RawMaterialPOHeader_POHeaderId",
                schema: "Purchase",
                table: "RawMaterialPODetail");

            migrationBuilder.RenameColumn(
                name: "POHeaderId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                newName: "RawMaterialPOId");

            migrationBuilder.RenameIndex(
                name: "IX_RawMaterialPODetail_POHeaderId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                newName: "IX_RawMaterialPODetail_RawMaterialPOId");

            migrationBuilder.AddForeignKey(
                name: "FK_RawMaterialPODetail_RawMaterialPOHeader_RawMaterialPOId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                column: "RawMaterialPOId",
                principalSchema: "Purchase",
                principalTable: "RawMaterialPOHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
