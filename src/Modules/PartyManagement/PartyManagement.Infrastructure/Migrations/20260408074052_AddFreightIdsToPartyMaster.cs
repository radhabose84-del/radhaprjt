using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFreightIdsToPartyMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseFreightId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesFreightId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseFreightId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "SalesFreightId",
                schema: "Party",
                table: "PartyMaster");
        }
    }
}
