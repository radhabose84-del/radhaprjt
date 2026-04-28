using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesLeadnumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeadNo",
                schema: "Sales",
                table: "SalesLead",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_LeadNo",
                schema: "Sales",
                table: "SalesLead",
                column: "LeadNo",
                unique: true,
                filter: "[LeadNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesLead_LeadNo",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropColumn(
                name: "LeadNo",
                schema: "Sales",
                table: "SalesLead");
        }
    }
}
