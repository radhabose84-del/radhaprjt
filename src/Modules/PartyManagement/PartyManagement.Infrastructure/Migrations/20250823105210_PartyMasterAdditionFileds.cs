using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyMasterAdditionFileds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CIN",
                schema: "Party",
                table: "PartyMaster",
                type: "nvarchar(25)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GSTRegistrationDate",
                schema: "Party",
                table: "PartyMaster",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IECode",
                schema: "Party",
                table: "PartyMaster",
                type: "nvarchar(25)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MSMERegistrationDate",
                schema: "Party",
                table: "PartyMaster",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CIN",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "GSTRegistrationDate",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "IECode",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "MSMERegistrationDate",
                schema: "Party",
                table: "PartyMaster");
        }
    }
}
