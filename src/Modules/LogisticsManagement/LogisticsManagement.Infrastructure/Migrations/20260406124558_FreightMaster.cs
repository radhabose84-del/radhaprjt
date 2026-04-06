using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FreightMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FreightMaster",
                schema: "Logistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FreightModeId = table.Column<int>(type: "int", nullable: false),
                    RateMethodId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreightMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FreightMaster_MiscMaster_FreightModeId",
                        column: x => x.FreightModeId,
                        principalSchema: "Logistics",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FreightMaster_MiscMaster_RateMethodId",
                        column: x => x.RateMethodId,
                        principalSchema: "Logistics",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FreightMaster_FreightModeId",
                schema: "Logistics",
                table: "FreightMaster",
                column: "FreightModeId");

            migrationBuilder.CreateIndex(
                name: "IX_FreightMaster_FreightModeId_RateMethodId",
                schema: "Logistics",
                table: "FreightMaster",
                columns: new[] { "FreightModeId", "RateMethodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FreightMaster_RateMethodId",
                schema: "Logistics",
                table: "FreightMaster",
                column: "RateMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreightMaster",
                schema: "Logistics");
        }
    }
}
