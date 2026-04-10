using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatchTypeAndFreightToDispatchAdvice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DispatchAddressId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DispatchTypeId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FreightId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_DispatchTypeId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchTypeMiscId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_FreightId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "FreightId");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchAdviceHeader_MiscMaster_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchTypeMiscId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchAdviceHeader_MiscMaster_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAdviceHeader_DispatchTypeId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAdviceHeader_DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropIndex(
                name: "IX_DispatchAdviceHeader_FreightId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropColumn(
                name: "DispatchTypeId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropColumn(
                name: "DispatchTypeMiscId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.DropColumn(
                name: "FreightId",
                schema: "Sales",
                table: "DispatchAdviceHeader");

            migrationBuilder.AlterColumn<int>(
                name: "DispatchAddressId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
