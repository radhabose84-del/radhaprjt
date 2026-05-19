using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBudgetGroupEmergencyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyPoApplicable",
                schema: "Budget",
                table: "BudgetGroup");

            migrationBuilder.DropColumn(
                name: "EmergencyPoLimit",
                schema: "Budget",
                table: "BudgetGroup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmergencyPoApplicable",
                schema: "Budget",
                table: "BudgetGroup",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "EmergencyPoLimit",
                schema: "Budget",
                table: "BudgetGroup",
                type: "decimal(18,2)",
                precision: 18,
                scale: 6,
                nullable: true);
        }
    }
}
