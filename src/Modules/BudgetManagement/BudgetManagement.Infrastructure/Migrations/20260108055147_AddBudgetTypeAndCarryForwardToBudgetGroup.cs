using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetTypeAndCarryForwardToBudgetGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetTypeId",
                schema: "Budget",
                table: "BudgetGroup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CarryForward",
                schema: "Budget",
                table: "BudgetGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetGroup_BudgetTypeId",
                schema: "Budget",
                table: "BudgetGroup",
                column: "BudgetTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetGroup_MiscMaster_BudgetTypeId",
                schema: "Budget",
                table: "BudgetGroup",
                column: "BudgetTypeId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetGroup_MiscMaster_BudgetTypeId",
                schema: "Budget",
                table: "BudgetGroup");

            migrationBuilder.DropIndex(
                name: "IX_BudgetGroup_BudgetTypeId",
                schema: "Budget",
                table: "BudgetGroup");

            migrationBuilder.DropColumn(
                name: "BudgetTypeId",
                schema: "Budget",
                table: "BudgetGroup");

            migrationBuilder.DropColumn(
                name: "CarryForward",
                schema: "Budget",
                table: "BudgetGroup");
        }
    }
}
