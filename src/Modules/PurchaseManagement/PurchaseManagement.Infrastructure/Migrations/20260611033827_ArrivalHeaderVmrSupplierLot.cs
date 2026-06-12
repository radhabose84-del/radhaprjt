using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArrivalHeaderVmrSupplierLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArrivalHeader_MiscMaster_QcStatusId",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.AddColumn<int>(
                name: "PRFrom",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PRTo",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierLotNo",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VmrId",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalHeader_VmrId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "VmrId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArrivalHeader_VmrId",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "PRFrom",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "PRTo",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "SupplierLotNo",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "VmrId",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.AddForeignKey(
                name: "FK_ArrivalHeader_MiscMaster_QcStatusId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "QcStatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
