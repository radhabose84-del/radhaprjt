using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceDetailLotId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LotNo",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.AddColumn<int>(
                name: "LotId",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_LotId",
                schema: "Sales",
                table: "InvoiceDetail",
                column: "LotId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetail_LotMaster_LotId",
                schema: "Sales",
                table: "InvoiceDetail",
                column: "LotId",
                principalSchema: "Sales",
                principalTable: "LotMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetail_LotMaster_LotId",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceDetail_LotId",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.DropColumn(
                name: "LotId",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.AddColumn<string>(
                name: "LotNo",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "varchar(50)",
                nullable: true);
        }
    }
}
