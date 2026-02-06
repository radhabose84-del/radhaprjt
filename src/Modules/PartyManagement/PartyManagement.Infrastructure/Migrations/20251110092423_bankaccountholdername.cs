using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class bankaccountholdername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "Party",
                table: "BankMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Party",
                table: "BankMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                schema: "Party",
                table: "BankAccount",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                schema: "Party",
                table: "BankMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                schema: "Party",
                table: "BankMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
