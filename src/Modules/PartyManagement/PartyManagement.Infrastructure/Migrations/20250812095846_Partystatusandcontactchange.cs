using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Partystatusandcontactchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartyStatus",
                schema: "Party",
                table: "PartyMaster",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactBy",
                schema: "Party",
                table: "PartyContact",
                type: "nvarchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartyStatus",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "ContactBy",
                schema: "Party",
                table: "PartyContact");
        }
    }
}
