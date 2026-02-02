using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeperformedid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceEntryActivities_PerformedById",
                schema: "Purchase",
                table: "ServiceEntryActivities");

            migrationBuilder.DropColumn(
                name: "PerformedById",
                schema: "Purchase",
                table: "ServiceEntryActivities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PerformedById",
                schema: "Purchase",
                table: "ServiceEntryActivities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntryActivities_PerformedById",
                schema: "Purchase",
                table: "ServiceEntryActivities",
                column: "PerformedById");
        }
    }
}
