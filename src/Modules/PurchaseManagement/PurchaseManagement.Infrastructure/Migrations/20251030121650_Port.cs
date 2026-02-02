using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Port : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "Purchase",
                table: "PortMaster");

            migrationBuilder.AlterColumn<int>(
                name: "PortTypeId",
                schema: "Purchase",
                table: "PortMaster",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                schema: "Purchase",
                table: "PortMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortMaster_PortTypeId",
                schema: "Purchase",
                table: "PortMaster",
                column: "PortTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PortMaster_TypeId",
                schema: "Purchase",
                table: "PortMaster",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PortMaster_MiscMaster_PortTypeId",
                schema: "Purchase",
                table: "PortMaster",
                column: "PortTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PortMaster_MiscMaster_TypeId",
                schema: "Purchase",
                table: "PortMaster",
                column: "TypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortMaster_MiscMaster_PortTypeId",
                schema: "Purchase",
                table: "PortMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_PortMaster_MiscMaster_TypeId",
                schema: "Purchase",
                table: "PortMaster");

            migrationBuilder.DropIndex(
                name: "IX_PortMaster_PortTypeId",
                schema: "Purchase",
                table: "PortMaster");

            migrationBuilder.DropIndex(
                name: "IX_PortMaster_TypeId",
                schema: "Purchase",
                table: "PortMaster");

            migrationBuilder.DropColumn(
                name: "TypeId",
                schema: "Purchase",
                table: "PortMaster");

            migrationBuilder.AlterColumn<int>(
                name: "PortTypeId",
                schema: "Purchase",
                table: "PortMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "Purchase",
                table: "PortMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
