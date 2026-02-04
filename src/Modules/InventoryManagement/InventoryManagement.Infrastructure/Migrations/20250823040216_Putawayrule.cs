using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Putawayrule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PutAwayStrategy_PriorityPerRule",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.CreateIndex(
                name: "UX_PutAwayStrategy_PriorityPerRule",
                schema: "Inventory",
                table: "PutAwayStrategy",
                columns: new[] { "PutAwayRuleId", "PriorityId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PutAwayStrategy_PriorityPerRule",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.CreateIndex(
                name: "UX_PutAwayStrategy_PriorityPerRule",
                schema: "Inventory",
                table: "PutAwayStrategy",
                columns: new[] { "PutAwayRuleId", "PriorityId" },
                unique: true);
        }
    }
}
