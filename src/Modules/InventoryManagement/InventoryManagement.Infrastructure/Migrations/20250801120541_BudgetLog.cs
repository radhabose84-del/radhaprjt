using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BudgetLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "Inventory",
                table: "BudgetLog",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "Inventory",
                table: "BudgetLog",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                schema: "Inventory",
                table: "BudgetLog",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "Inventory",
                table: "BudgetLog",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLog_ActionTypeId",
                schema: "Inventory",
                table: "BudgetLog",
                column: "ActionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetLog_MiscMaster_ActionTypeId",
                schema: "Inventory",
                table: "BudgetLog",
                column: "ActionTypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetLog_MiscMaster_ActionTypeId",
                schema: "Inventory",
                table: "BudgetLog");

            migrationBuilder.DropIndex(
                name: "IX_BudgetLog_ActionTypeId",
                schema: "Inventory",
                table: "BudgetLog");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Inventory",
                table: "BudgetLog");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "Inventory",
                table: "BudgetLog");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "Inventory",
                table: "BudgetLog");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "Inventory",
                table: "BudgetLog");
        }
    }
}
