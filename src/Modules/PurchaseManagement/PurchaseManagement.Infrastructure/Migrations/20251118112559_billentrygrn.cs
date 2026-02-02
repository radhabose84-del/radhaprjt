using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class billentrygrn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SSGTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                newName: "SGSTTotal");

            migrationBuilder.RenameColumn(
                name: "ISGTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                newName: "IGSTTotal");

            migrationBuilder.RenameColumn(
                name: "CSGTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                newName: "CGSTTotal");

            migrationBuilder.AlterColumn<int>(
                name: "PoDetailId",
                schema: "Purchase",
                table: "PurchaseBillEntryDetail",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "UOMId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SGSTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                newName: "SSGTTotal");

            migrationBuilder.RenameColumn(
                name: "IGSTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                newName: "ISGTTotal");

            migrationBuilder.RenameColumn(
                name: "CGSTTotal",
                schema: "Purchase",
                table: "GrnHeader",
                newName: "CSGTTotal");

            migrationBuilder.AlterColumn<decimal>(
                name: "PoDetailId",
                schema: "Purchase",
                table: "PurchaseBillEntryDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "UOMId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
