using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesOrderTypeMasterAndEnquiryAndRefnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComplaintRefno",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPoRefno",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesEnquiryHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesOrderTypeMasterId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesEnquiryHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesEnquiryHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesOrderTypeMasterId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesOrderTypeMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_SalesEnquiryHeader_SalesEnquiryHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesEnquiryHeaderId",
                principalSchema: "Sales",
                principalTable: "SalesEnquiryHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_SalesOrderTypeMaster_SalesOrderTypeMasterId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesOrderTypeMasterId",
                principalSchema: "Sales",
                principalTable: "SalesOrderTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_SalesEnquiryHeader_SalesEnquiryHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_SalesOrderTypeMaster_SalesOrderTypeMasterId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_SalesEnquiryHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_SalesOrderTypeMasterId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ComplaintRefno",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "CustomerPoRefno",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "SalesEnquiryHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "SalesOrderTypeMasterId",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
