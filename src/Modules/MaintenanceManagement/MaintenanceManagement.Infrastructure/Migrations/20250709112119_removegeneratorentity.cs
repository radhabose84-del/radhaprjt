using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removegeneratorentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Generator",
                schema: "Maintenance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Generator",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Current = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Frequency = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    FuelTankCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GenSetName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    GensetStatusId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    KVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultiplicationFactor = table.Column<int>(type: "int", nullable: false),
                    OpeningEnergyReading = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Power = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PowerFactor = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    RPM = table.Column<int>(type: "int", nullable: false),
                    Serialnumber = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Voltage = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generator", x => x.Id);
                });
        }
    }
}
