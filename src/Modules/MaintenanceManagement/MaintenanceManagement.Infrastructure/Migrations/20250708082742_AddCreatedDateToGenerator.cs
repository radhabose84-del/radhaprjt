using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedDateToGenerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Generator",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    GenSetName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Serialnumber = table.Column<int>(type: "int", nullable: false),
                    KVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Current = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Voltage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Power = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RPM = table.Column<int>(type: "int", nullable: false),
                    PowerFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MultiplicationFactor = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FuelTankCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GensetStatus = table.Column<int>(type: "int", nullable: false),
                    GensetStatusTypeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Generator_MiscMaster_GensetStatusTypeId",
                        column: x => x.GensetStatusTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Generator_GensetStatusTypeId",
                schema: "Maintenance",
                table: "Generator",
                column: "GensetStatusTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Generator",
                schema: "Maintenance");
        }
    }
}
