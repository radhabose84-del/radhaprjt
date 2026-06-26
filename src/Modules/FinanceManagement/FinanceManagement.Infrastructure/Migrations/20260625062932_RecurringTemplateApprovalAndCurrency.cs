using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecurringTemplateApprovalAndCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalSavedFilter",
                schema: "Finance");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail",
                type: "decimal(18,6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail");

            migrationBuilder.CreateTable(
                name: "JournalSavedFilter",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalSavedFilter", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalSavedFilter_UserId",
                schema: "Finance",
                table: "JournalSavedFilter",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_JournalSavedFilter_UserName",
                schema: "Finance",
                table: "JournalSavedFilter",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }
    }
}
