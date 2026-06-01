using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInwardHdr_VehicleMovementRecordId_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VehicleMovementRecordId",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VehicleMovementRecordId",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
