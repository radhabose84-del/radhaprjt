using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BankAccount_OwnerTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerTypeId",
                schema: "Party",
                table: "BankAccount",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_OwnerTypeId",
                schema: "Party",
                table: "BankAccount",
                column: "OwnerTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccount_MiscMaster_OwnerTypeId",
                schema: "Party",
                table: "BankAccount",
                column: "OwnerTypeId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccount_MiscMaster_OwnerTypeId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropIndex(
                name: "IX_BankAccount_OwnerTypeId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "OwnerTypeId",
                schema: "Party",
                table: "BankAccount");
        }
    }
}
