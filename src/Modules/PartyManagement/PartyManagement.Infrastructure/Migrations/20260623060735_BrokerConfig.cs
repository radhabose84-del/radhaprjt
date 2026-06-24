using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BrokerConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrokerConfig",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    SettlementCycleId = table.Column<int>(type: "int", nullable: true),
                    TdsApplicable = table.Column<byte>(type: "tinyint", nullable: false),
                    TdsCode = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    DefaultCommissionGl = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    AgreementStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AgreementEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BrokerPayableControlGl = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    TargetAmount = table.Column<decimal>(type: "decimal(15,3)", precision: 18, scale: 6, nullable: true),
                    TargetPeriod = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrokerConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrokerConfig_MiscMaster_SettlementCycleId",
                        column: x => x.SettlementCycleId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrokerConfig_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerConfig_PartyId",
                schema: "Party",
                table: "BrokerConfig",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerConfig_SettlementCycleId",
                schema: "Party",
                table: "BrokerConfig",
                column: "SettlementCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrokerConfig",
                schema: "Party");
        }
    }
}
