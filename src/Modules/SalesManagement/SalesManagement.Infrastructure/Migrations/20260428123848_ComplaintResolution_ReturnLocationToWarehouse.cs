using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ComplaintResolution_ReturnLocationToWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplaintResolution_MiscMaster_ReturnLocationId",
                schema: "Sales",
                table: "ComplaintResolution");

            migrationBuilder.DropIndex(
                name: "IX_ComplaintResolution_ReturnLocationId",
                schema: "Sales",
                table: "ComplaintResolution");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ComplaintResolution_ReturnLocationId",
                schema: "Sales",
                table: "ComplaintResolution",
                column: "ReturnLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplaintResolution_MiscMaster_ReturnLocationId",
                schema: "Sales",
                table: "ComplaintResolution",
                column: "ReturnLocationId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
