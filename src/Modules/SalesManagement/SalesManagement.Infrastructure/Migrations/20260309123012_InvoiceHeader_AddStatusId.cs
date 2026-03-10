using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceHeader_AddStatusId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Sales",
                table: "InvoiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_StatusId",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceHeader_MiscMaster_StatusId",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "StatusId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceHeader_MiscMaster_StatusId",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceHeader_StatusId",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Sales",
                table: "InvoiceHeader");
        }
    }
}
