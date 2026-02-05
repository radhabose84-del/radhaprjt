using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BudgetMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "BudgetMaster",
                schema: "Budget",
                newName: "BudgetMaster",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "BudgetLog",
                schema: "Budget",
                newName: "BudgetLog",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "BudgetDetail",
                schema: "Budget",
                newName: "BudgetDetail",
                newSchema: "Inventory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Budget");

            migrationBuilder.RenameTable(
                name: "BudgetMaster",
                schema: "Inventory",
                newName: "BudgetMaster",
                newSchema: "Budget");

            migrationBuilder.RenameTable(
                name: "BudgetLog",
                schema: "Inventory",
                newName: "BudgetLog",
                newSchema: "Budget");

            migrationBuilder.RenameTable(
                name: "BudgetDetail",
                schema: "Inventory",
                newName: "BudgetDetail",
                newSchema: "Budget");
        }
    }
}
