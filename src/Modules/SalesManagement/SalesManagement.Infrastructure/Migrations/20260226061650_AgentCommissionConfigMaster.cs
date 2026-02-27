using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgentCommissionConfigMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentCommissionConfig",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    SalesSegmentId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CommissionTypeId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    SubAgentPercentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    ValidityFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidityTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    table.PrimaryKey("PK_AgentCommissionConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCommissionConfig_MiscMaster_CommissionTypeId",
                        column: x => x.CommissionTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentCommissionConfig_SalesSegment_SalesSegmentId",
                        column: x => x.SalesSegmentId,
                        principalSchema: "Sales",
                        principalTable: "SalesSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_AgentId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_AgentId_SalesSegmentId_ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                columns: new[] { "AgentId", "SalesSegmentId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_CommissionTypeId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "CommissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "SalesSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_ValidityFrom_ValidityTo",
                schema: "Sales",
                table: "AgentCommissionConfig",
                columns: new[] { "ValidityFrom", "ValidityTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentCommissionConfig",
                schema: "Sales");
        }
    }
}
