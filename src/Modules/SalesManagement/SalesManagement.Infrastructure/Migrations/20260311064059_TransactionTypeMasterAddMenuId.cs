using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransactionTypeMasterAddMenuId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MenuId",
                schema: "Finance",
                table: "TransactionTypeMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_MenuId",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "MenuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionTypeMaster_MenuId",
                schema: "Finance",
                table: "TransactionTypeMaster");

            migrationBuilder.DropColumn(
                name: "MenuId",
                schema: "Finance",
                table: "TransactionTypeMaster");
        }
    }
}
