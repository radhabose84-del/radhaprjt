using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class budgetrequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MiscTypeMaster",
                schema: "Purchase",
                newName: "MiscTypeMaster",
                newSchema: "Budget");

            migrationBuilder.RenameTable(
                name: "MiscMaster",
                schema: "Purchase",
                newName: "MiscMaster",
                newSchema: "Budget");

            migrationBuilder.RenameTable(
                name: "ActivityLog",
                schema: "Purchase",
                newName: "ActivityLog",
                newSchema: "Budget");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Purchase");

            migrationBuilder.RenameTable(
                name: "MiscTypeMaster",
                schema: "Budget",
                newName: "MiscTypeMaster",
                newSchema: "Purchase");

            migrationBuilder.RenameTable(
                name: "MiscMaster",
                schema: "Budget",
                newName: "MiscMaster",
                newSchema: "Purchase");

            migrationBuilder.RenameTable(
                name: "ActivityLog",
                schema: "Budget",
                newName: "ActivityLog",
                newSchema: "Purchase");
        }
    }
}
