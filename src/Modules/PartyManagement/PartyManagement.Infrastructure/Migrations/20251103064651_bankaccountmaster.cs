using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class bankaccountmaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankMaster_MiscMaster_AccountTypeId",
                schema: "Party",
                table: "BankMaster");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankMaster",
                schema: "Party",
                table: "BankMaster");

            migrationBuilder.RenameTable(
                name: "BankMaster",
                schema: "Party",
                newName: "BankAccount",
                newSchema: "Party");

            migrationBuilder.RenameIndex(
                name: "IX_BankMaster_AccountTypeId",
                schema: "Party",
                table: "BankAccount",
                newName: "IX_BankAccount_AccountTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankAccount",
                schema: "Party",
                table: "BankAccount",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccount_MiscMaster_AccountTypeId",
                schema: "Party",
                table: "BankAccount",
                column: "AccountTypeId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccount_MiscMaster_AccountTypeId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankAccount",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.RenameTable(
                name: "BankAccount",
                schema: "Party",
                newName: "BankMaster",
                newSchema: "Party");

            migrationBuilder.RenameIndex(
                name: "IX_BankAccount_AccountTypeId",
                schema: "Party",
                table: "BankMaster",
                newName: "IX_BankMaster_AccountTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankMaster",
                schema: "Party",
                table: "BankMaster",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankMaster_MiscMaster_AccountTypeId",
                schema: "Party",
                table: "BankMaster",
                column: "AccountTypeId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
