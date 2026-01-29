using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class feedertbladd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feeder",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeederCode = table.Column<string>(type: "varchar(50)", nullable: false),
                    FeederName = table.Column<string>(type: "varchar(100)", nullable: false),
                    FeederGroupId = table.Column<int>(type: "int", nullable: false),
                    FeederTypeId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: false),
                    MultiplicationFactor = table.Column<decimal>(type: "Decimal(18,2)", nullable: false),
                    EffectiveDate = table.Column<DateTimeOffset>(type: "DateTimeOffset", nullable: false),
                    OpeningReading = table.Column<decimal>(type: "Decimal(18,2)", nullable: false),
                    HighPriority = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Feeder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feeder_FeederGroup_FeederGroupId",
                        column: x => x.FeederGroupId,
                        principalSchema: "Maintenance",
                        principalTable: "FeederGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feeder_MiscMaster_FeederTypeId",
                        column: x => x.FeederTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feeder_FeederGroupId",
                schema: "Maintenance",
                table: "Feeder",
                column: "FeederGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Feeder_FeederTypeId",
                schema: "Maintenance",
                table: "Feeder",
                column: "FeederTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feeder",
                schema: "Maintenance");
        }
    }
}
