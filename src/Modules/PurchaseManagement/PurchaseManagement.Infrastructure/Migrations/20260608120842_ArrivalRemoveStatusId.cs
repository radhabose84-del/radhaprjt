using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArrivalRemoveStatusId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArrivalHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropIndex(
                name: "IX_ArrivalHeader_StatusId",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "ArrivalHeader");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalHeader_StatusId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArrivalHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "StatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
