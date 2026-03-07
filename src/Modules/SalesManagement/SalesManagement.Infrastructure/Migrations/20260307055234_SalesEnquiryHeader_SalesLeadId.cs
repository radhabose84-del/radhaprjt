using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesEnquiryHeader_SalesLeadId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesLeadId",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesEnquiryHeader_SalesLeadId",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                column: "SalesLeadId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesEnquiryHeader_SalesLead_SalesLeadId",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                column: "SalesLeadId",
                principalSchema: "Sales",
                principalTable: "SalesLead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesEnquiryHeader_SalesLead_SalesLeadId",
                schema: "Sales",
                table: "SalesEnquiryHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesEnquiryHeader_SalesLeadId",
                schema: "Sales",
                table: "SalesEnquiryHeader");

            migrationBuilder.DropColumn(
                name: "SalesLeadId",
                schema: "Sales",
                table: "SalesEnquiryHeader");
        }
    }
}
