using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addcolumnmetertypeandavailablity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MeterAvailable",
                schema: "Maintenance",
                table: "Feeder",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MeterTypeId",
                schema: "Maintenance",
                table: "Feeder",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeterAvailable",
                schema: "Maintenance",
                table: "Feeder");

            migrationBuilder.DropColumn(
                name: "MeterTypeId",
                schema: "Maintenance",
                table: "Feeder");
        }
    }
}
