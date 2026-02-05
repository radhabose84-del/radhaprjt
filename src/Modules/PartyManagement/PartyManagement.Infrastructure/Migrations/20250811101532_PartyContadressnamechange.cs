using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyContadressnamechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GSTNumber",
                schema: "Party",
                table: "PartyContact",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "AddressLine6",
                schema: "Party",
                table: "PartyAddress",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "AddressLine5",
                schema: "Party",
                table: "PartyAddress",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "AddressLine4",
                schema: "Party",
                table: "PartyAddress",
                newName: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstName",
                schema: "Party",
                table: "PartyContact",
                newName: "GSTNumber");

            migrationBuilder.RenameColumn(
                name: "State",
                schema: "Party",
                table: "PartyAddress",
                newName: "AddressLine4");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                schema: "Party",
                table: "PartyAddress",
                newName: "AddressLine5");

            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "Party",
                table: "PartyAddress",
                newName: "AddressLine6");
        }
    }
}
