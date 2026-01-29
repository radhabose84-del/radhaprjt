using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratoraddeffectivedate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingEnergyReading",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EffectiveDate",
                schema: "Maintenance",
                table: "Generator",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                schema: "Maintenance",
                table: "Generator");

            migrationBuilder.AddColumn<decimal>(
                name: "ClosingEnergyReading",
                schema: "Maintenance",
                table: "Generator",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
