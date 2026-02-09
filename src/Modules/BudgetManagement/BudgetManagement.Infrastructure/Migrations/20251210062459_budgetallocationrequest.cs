using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class budgetallocationrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetAllocation_BudgetRequest_RequestTypeId",
                schema: "Budget",
                table: "BudgetAllocation");

            migrationBuilder.RenameColumn(
                name: "RequestTypeId",
                schema: "Budget",
                table: "BudgetAllocation",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetAllocation_RequestTypeId",
                schema: "Budget",
                table: "BudgetAllocation",
                newName: "IX_BudgetAllocation_RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetAllocation_BudgetRequest_RequestId",
                schema: "Budget",
                table: "BudgetAllocation",
                column: "RequestId",
                principalSchema: "Budget",
                principalTable: "BudgetRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetAllocation_BudgetRequest_RequestId",
                schema: "Budget",
                table: "BudgetAllocation");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                schema: "Budget",
                table: "BudgetAllocation",
                newName: "RequestTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetAllocation_RequestId",
                schema: "Budget",
                table: "BudgetAllocation",
                newName: "IX_BudgetAllocation_RequestTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetAllocation_BudgetRequest_RequestTypeId",
                schema: "Budget",
                table: "BudgetAllocation",
                column: "RequestTypeId",
                principalSchema: "Budget",
                principalTable: "BudgetRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
