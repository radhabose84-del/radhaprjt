using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecurringTemplateUnitId_DropAutoApproved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoApproved",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader");

            migrationBuilder.AddColumn<bool>(
                name: "AutoApproved",
                schema: "Finance",
                table: "JournalHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
