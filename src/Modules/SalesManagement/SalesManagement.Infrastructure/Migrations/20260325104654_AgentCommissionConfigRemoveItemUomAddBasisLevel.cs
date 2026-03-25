using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgentCommissionConfigRemoveItemUomAddBasisLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_AgentId_SalesSegmentId_ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropColumn(
                name: "ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropColumn(
                name: "SubAgentPercentage",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.RenameColumn(
                name: "UomId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "CommissionBasisId");

            migrationBuilder.AddColumn<int>(
                name: "ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_AgentId_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                columns: new[] { "AgentId", "SalesSegmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "ApplicableLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "CommissionBasisId");

            // Fix existing rows: UomId (cross-module FK to InventoryManagement) was renamed to
            // CommissionBasisId (same-module FK to MiscMaster) — ALL old values are invalid
            migrationBuilder.Sql("UPDATE Sales.AgentCommissionConfig SET CommissionBasisId = NULL WHERE CommissionBasisId IS NOT NULL;");
            migrationBuilder.Sql("UPDATE Sales.AgentCommissionConfig SET ApplicableLevelId = NULL WHERE ApplicableLevelId IS NOT NULL;");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "ApplicableLevelId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "CommissionBasisId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_AgentId_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropColumn(
                name: "ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.RenameColumn(
                name: "CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "UomId");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SubAgentPercentage",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_AgentId_SalesSegmentId_ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                columns: new[] { "AgentId", "SalesSegmentId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_ItemId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "ItemId");
        }
    }
}
