using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentCommissionFieldsToSalesOrderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgentPaymentTermsId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionValue",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "AgentCommissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "AgentCommissionSlabId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_AgentPaymentTermsId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "AgentPaymentTermsId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_AgentCommissionConfig_AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "AgentCommissionId",
                principalSchema: "Sales",
                principalTable: "AgentCommissionConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_AgentCommissionSlab_AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "AgentCommissionSlabId",
                principalSchema: "Sales",
                principalTable: "AgentCommissionSlab",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_AgentCommissionConfig_AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_AgentCommissionSlab_AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_AgentPaymentTermsId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "AgentPaymentTermsId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "CommissionValue",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
