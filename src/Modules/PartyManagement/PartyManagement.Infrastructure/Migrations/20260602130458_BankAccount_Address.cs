using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankAccount_Address : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankAccountId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "Party",
                table: "BankAccount",
                type: "nvarchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "Party",
                table: "BankAccount",
                type: "nvarchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                schema: "Party",
                table: "BankAccount",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                schema: "Party",
                table: "BankAccount",
                type: "nvarchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                schema: "Party",
                table: "BankAccount",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "Pincode",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "StateId",
                schema: "Party",
                table: "BankAccount");
        }
    }
}
