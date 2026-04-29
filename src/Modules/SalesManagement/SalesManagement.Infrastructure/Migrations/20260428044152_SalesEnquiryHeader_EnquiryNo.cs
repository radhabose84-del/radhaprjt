using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesEnquiryHeader_EnquiryNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnquiryNo",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesEnquiryHeader_EnquiryNo",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                column: "EnquiryNo",
                unique: true,
                filter: "[EnquiryNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesEnquiryHeader_EnquiryNo",
                schema: "Sales",
                table: "SalesEnquiryHeader");

            migrationBuilder.DropColumn(
                name: "EnquiryNo",
                schema: "Sales",
                table: "SalesEnquiryHeader");
        }
    }
}
