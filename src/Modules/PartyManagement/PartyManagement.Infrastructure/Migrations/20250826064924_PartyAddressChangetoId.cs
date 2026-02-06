using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyAddressChangetoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                schema: "Party",
                table: "PartyAddress");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "Party",
                table: "PartyAddress");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "Party",
                table: "PartyAddress");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                schema: "Party",
                table: "PartyAddress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                schema: "Party",
                table: "PartyAddress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                schema: "Party",
                table: "PartyAddress",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "Party",
                table: "PartyAddress");

            migrationBuilder.DropColumn(
                name: "CountryId",
                schema: "Party",
                table: "PartyAddress");

            migrationBuilder.DropColumn(
                name: "StateId",
                schema: "Party",
                table: "PartyAddress");

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "Party",
                table: "PartyAddress",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "Party",
                table: "PartyAddress",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "Party",
                table: "PartyAddress",
                type: "nvarchar(50)",
                nullable: true);
        }
    }
}
