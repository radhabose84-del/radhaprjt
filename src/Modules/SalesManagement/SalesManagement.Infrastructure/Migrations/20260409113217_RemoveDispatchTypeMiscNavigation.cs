using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDispatchTypeMiscNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchAdviceHeader_MiscMaster_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAdviceHeader_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropColumn(
                name: "DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            // FK constraint skipped — existing rows have DispatchTypeId = 0
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchTypeMiscId");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchAdviceHeader_MiscMaster_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchTypeMiscId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
