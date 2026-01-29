using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PowerConsumptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PowerConsumption",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeederTypeId = table.Column<int>(type: "int", nullable: false),
                    FeederId = table.Column<int>(type: "int", nullable: false),
                    OpeningReading = table.Column<decimal>(type: "Decimal(18,3)", nullable: false),
                    ClosingReading = table.Column<decimal>(type: "Decimal(18,3)", nullable: false),
                    TotalUnits = table.Column<decimal>(type: "Decimal(18,3)", nullable: false),
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
                    table.PrimaryKey("PK_PowerConsumption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerConsumption_Feeder_FeederId",
                        column: x => x.FeederId,
                        principalSchema: "Maintenance",
                        principalTable: "Feeder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PowerConsumption_MiscMaster_FeederTypeId",
                        column: x => x.FeederTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PowerConsumption_FeederId",
                schema: "Maintenance",
                table: "PowerConsumption",
                column: "FeederId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerConsumption_FeederTypeId",
                schema: "Maintenance",
                table: "PowerConsumption",
                column: "FeederTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerConsumption",
                schema: "Maintenance");
        }
    }
}
