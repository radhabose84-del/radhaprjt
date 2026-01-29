using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GensetConsumptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratorConsumption",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratorId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RunningHours = table.Column<decimal>(type: "Decimal(18,3)", nullable: false),
                    DieselConsumption = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    OpeningEnergyReading = table.Column<decimal>(type: "Decimal(18,3)", nullable: false),
                    ClosingEnergyReading = table.Column<decimal>(type: "Decimal(18,3)", nullable: false),
                    Energy = table.Column<decimal>(type: "Decimal(18,3)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    PurposeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(max)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratorConsumption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratorConsumption_Generator_GeneratorId",
                        column: x => x.GeneratorId,
                        principalSchema: "Maintenance",
                        principalTable: "Generator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneratorConsumption_MiscMaster_PurposeId",
                        column: x => x.PurposeId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratorConsumption_GeneratorId",
                schema: "Maintenance",
                table: "GeneratorConsumption",
                column: "GeneratorId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratorConsumption_PurposeId",
                schema: "Maintenance",
                table: "GeneratorConsumption",
                column: "PurposeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratorConsumption",
                schema: "Maintenance");
        }
    }
}
