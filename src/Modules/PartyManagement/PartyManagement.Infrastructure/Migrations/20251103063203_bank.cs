using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class bank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BankMaster_AccountTypeId",
                schema: "Party",
                table: "BankMaster",
                column: "AccountTypeId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankMaster_MiscMaster_AccountTypeId",
                schema: "Party",
                table: "BankMaster");

            migrationBuilder.DropIndex(
                name: "IX_BankMaster_AccountTypeId",
                schema: "Party",
                table: "BankMaster");
        }
    }
}
