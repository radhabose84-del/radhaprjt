using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesLeadMarketingOfficer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesLead_MarketingOfficer_MarketingPersonId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.RenameColumn(
                name: "MarketingPersonId",
                schema: "Sales",
                table: "SalesLead",
                newName: "MarketingOfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesLead_MarketingOfficer_MarketingOfficerId",
                schema: "Sales",
                table: "SalesLead",
                column: "MarketingOfficerId",
                principalSchema: "Sales",
                principalTable: "MarketingOfficer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesLead_MarketingOfficer_MarketingOfficerId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.RenameColumn(
                name: "MarketingOfficerId",
                schema: "Sales",
                table: "SalesLead",
                newName: "MarketingPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesLead_MarketingOfficer_MarketingPersonId",
                schema: "Sales",
                table: "SalesLead",
                column: "MarketingPersonId",
                principalSchema: "Sales",
                principalTable: "MarketingOfficer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
