using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesEnquiryDetail_VariantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                schema: "Sales",
                table: "SalesEnquiryDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesEnquiryDetail_VariantId",
                schema: "Sales",
                table: "SalesEnquiryDetail",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesEnquiryDetail_VariantId",
                schema: "Sales",
                table: "SalesEnquiryDetail");

            migrationBuilder.DropColumn(
                name: "VariantId",
                schema: "Sales",
                table: "SalesEnquiryDetail");
        }
    }
}
