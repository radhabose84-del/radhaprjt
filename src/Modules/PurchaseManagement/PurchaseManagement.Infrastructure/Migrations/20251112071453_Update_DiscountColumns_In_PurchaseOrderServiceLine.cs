using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_DiscountColumns_In_PurchaseOrderServiceLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScheduleID",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                newName: "ScheduleId");

            migrationBuilder.AlterColumn<string>(
                name: "DiscountType",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountId",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine",
                type: "int",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceCode",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountId",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine");

            migrationBuilder.DropColumn(
                name: "ServiceCode",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                newName: "ScheduleID");

            migrationBuilder.AlterColumn<string>(
                name: "DiscountType",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
