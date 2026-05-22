using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VMR_DropReceivingType_RelaxFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "DriverName",
                schema: "Gate",
                table: "VehicleMovementRecord",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "DriverMobileNo",
                schema: "Gate",
                table: "VehicleMovementRecord",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AddColumn<string>(
                name: "CourierNumber",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceivingTypeId",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardHdr_ReceivingTypeId",
                schema: "Gate",
                table: "GateInwardHdr",
                column: "ReceivingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GateInwardHdr_MiscMaster_ReceivingTypeId",
                schema: "Gate",
                table: "GateInwardHdr",
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
                name: "FK_GateInwardHdr_MiscMaster_ReceivingTypeId",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropIndex(
                name: "IX_GateInwardHdr_ReceivingTypeId",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "CourierNumber",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "ReceivingTypeId",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.AlterColumn<string>(
                name: "DriverName",
                schema: "Gate",
                table: "VehicleMovementRecord",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DriverMobileNo",
                schema: "Gate",
                table: "VehicleMovementRecord",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);

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
    }
}
