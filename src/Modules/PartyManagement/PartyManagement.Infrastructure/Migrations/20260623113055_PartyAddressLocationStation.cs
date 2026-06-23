using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyAddressLocationStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                schema: "Party",
                table: "PartyAddress",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationId",
                schema: "Party",
                table: "PartyAddress",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "Party",
                table: "PartyAddress");

            migrationBuilder.DropColumn(
                name: "StationId",
                schema: "Party",
                table: "PartyAddress");
        }
    }
}
