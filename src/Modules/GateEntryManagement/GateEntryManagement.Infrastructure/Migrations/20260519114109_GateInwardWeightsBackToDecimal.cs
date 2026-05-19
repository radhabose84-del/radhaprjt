using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInwardWeightsBackToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TareWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GrossWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "decimal(10,3)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TareWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NetWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GrossWeight",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
