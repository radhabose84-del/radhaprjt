using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class unitmigrationIsdeletedColumnadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitContacts_UnitId",
                schema: "AppData",
                table: "UnitContacts");

            migrationBuilder.DropIndex(
                name: "IX_UnitAddress_UnitId",
                schema: "AppData",
                table: "UnitAddress");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "AppData",
                table: "Unit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UnitContacts_UnitId",
                schema: "AppData",
                table: "UnitContacts",
                column: "UnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitAddress_UnitId",
                schema: "AppData",
                table: "UnitAddress",
                column: "UnitId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitContacts_UnitId",
                schema: "AppData",
                table: "UnitContacts");

            migrationBuilder.DropIndex(
                name: "IX_UnitAddress_UnitId",
                schema: "AppData",
                table: "UnitAddress");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.CreateIndex(
                name: "IX_UnitContacts_UnitId",
                schema: "AppData",
                table: "UnitContacts",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitAddress_UnitId",
                schema: "AppData",
                table: "UnitAddress",
                column: "UnitId");
        }
    }
}
