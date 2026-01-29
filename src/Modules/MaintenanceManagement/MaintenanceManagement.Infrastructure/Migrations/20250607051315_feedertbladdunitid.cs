using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class feedertbladdunitid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Maintenance",
                table: "FeederGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningReading",
                schema: "Maintenance",
                table: "Feeder",
                type: "Decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Maintenance",
                table: "Feeder",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Maintenance",
                table: "FeederGroup");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Maintenance",
                table: "Feeder");

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningReading",
                schema: "Maintenance",
                table: "Feeder",
                type: "Decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "Decimal(18,3)");
        }
    }
}
