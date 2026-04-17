using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceSalesSegmentWithSalesGroup_AgentCustomerMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCustomerMapping_SalesSegment_SalesSegmentId",
                schema: "Sales",
                table: "AgentCustomerMapping");

            migrationBuilder.RenameColumn(
                name: "SalesSegmentId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                newName: "SalesGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_AgentCustomerMapping_SalesSegmentId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                newName: "IX_AgentCustomerMapping_SalesGroupId");

            // Data backfill: existing rows carry SalesSegment IDs (not valid SalesGroup IDs).
            // Set all existing rows to SalesGroupId = 30 (Hosiery) so the new FK constraint succeeds.
            // Team will correct each row via the Update API post-migration.
            migrationBuilder.Sql(@"UPDATE Sales.AgentCustomerMapping SET SalesGroupId = 30;");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCustomerMapping_SalesGroup_SalesGroupId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                column: "SalesGroupId",
                principalSchema: "Sales",
                principalTable: "SalesGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCustomerMapping_SalesGroup_SalesGroupId",
                schema: "Sales",
                table: "AgentCustomerMapping");

            migrationBuilder.RenameColumn(
                name: "SalesGroupId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                newName: "SalesSegmentId");

            migrationBuilder.RenameIndex(
                name: "IX_AgentCustomerMapping_SalesGroupId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                newName: "IX_AgentCustomerMapping_SalesSegmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCustomerMapping_SalesSegment_SalesSegmentId",
                schema: "Sales",
                table: "AgentCustomerMapping",
                column: "SalesSegmentId",
                principalSchema: "Sales",
                principalTable: "SalesSegment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
