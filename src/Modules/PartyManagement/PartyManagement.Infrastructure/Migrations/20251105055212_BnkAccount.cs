using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BnkAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankCode",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "BankName",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "Branch",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.AddColumn<int>(
                name: "BankId",
                schema: "Party",
                table: "BankAccount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                schema: "Party",
                table: "BankAccount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BankMaster",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankMaster", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_BranchId",
                schema: "Party",
                table: "BankAccount",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BankMaster_BankCode",
                schema: "Party",
                table: "BankMaster",
                column: "BankCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccount_MiscMaster_BranchId",
                schema: "Party",
                table: "BankAccount",
                column: "BranchId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccount_MiscMaster_BranchId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropTable(
                name: "BankMaster",
                schema: "Party");

            migrationBuilder.DropIndex(
                name: "IX_BankAccount_BranchId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "BankId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Party",
                table: "BankAccount");

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                schema: "Party",
                table: "BankAccount",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                schema: "Party",
                table: "BankAccount",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                schema: "Party",
                table: "BankAccount",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }
    }
}
