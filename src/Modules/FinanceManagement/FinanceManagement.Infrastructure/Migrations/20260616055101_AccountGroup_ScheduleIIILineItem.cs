using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AccountGroup_ScheduleIIILineItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountTypeId",
                schema: "Finance",
                table: "AccountGroup",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_AccountTypeId",
                schema: "Finance",
                table: "AccountGroup",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup",
                column: "ScheduleIIILineItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountGroup_AccountTypeMaster_AccountTypeId",
                schema: "Finance",
                table: "AccountGroup",
                column: "AccountTypeId",
                principalSchema: "Finance",
                principalTable: "AccountTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccountGroup_ScheduleIIILineItem_ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup",
                column: "ScheduleIIILineItemId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIILineItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountGroup_AccountTypeMaster_AccountTypeId",
                schema: "Finance",
                table: "AccountGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountGroup_ScheduleIIILineItem_ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup");

            migrationBuilder.DropIndex(
                name: "IX_AccountGroup_AccountTypeId",
                schema: "Finance",
                table: "AccountGroup");

            migrationBuilder.DropIndex(
                name: "IX_AccountGroup_ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup");

            migrationBuilder.DropColumn(
                name: "AccountTypeId",
                schema: "Finance",
                table: "AccountGroup");

            migrationBuilder.DropColumn(
                name: "ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup");
        }
    }
}
