using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VMR_AddReceivingTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VehicleNumber",
                schema: "Gate",
                table: "VehicleMovementRecord",
                type: "varchar(20)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AddColumn<int>(
                name: "ReceivingTypeId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMovementRecord_ReceivingTypeId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "ReceivingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleMovementRecord_MiscMaster_ReceivingTypeId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "ReceivingTypeId",
                principalSchema: "Gate",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleMovementRecord_MiscMaster_ReceivingTypeId",
                schema: "Gate",
                table: "VehicleMovementRecord");

            migrationBuilder.DropIndex(
                name: "IX_VehicleMovementRecord_ReceivingTypeId",
                schema: "Gate",
                table: "VehicleMovementRecord");

            migrationBuilder.DropColumn(
                name: "ReceivingTypeId",
                schema: "Gate",
                table: "VehicleMovementRecord");

            migrationBuilder.AlterColumn<string>(
                name: "VehicleNumber",
                schema: "Gate",
                table: "VehicleMovementRecord",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldNullable: true);
        }
    }
}
