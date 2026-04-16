using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountSnapshotToSalesOrderAmendment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgentPaymentTermsId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MdDiscountValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscountValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AgentCommissionPercentage",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesOrderAmendmentDiscount",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderAmendmentHeaderId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDiscountId = table.Column<int>(type: "int", nullable: true),
                    DiscountMasterId = table.Column<int>(type: "int", nullable: false),
                    SlabTypeId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    DiscountSlabId = table.Column<int>(type: "int", nullable: true),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true),
                    TotalDiscountValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderAmendmentDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderAmendmentDiscount_SalesOrderAmendmentHeader_SalesOrderAmendmentHeaderId",
                        column: x => x.SalesOrderAmendmentHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderAmendmentHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentDiscount_DiscountMasterId",
                schema: "Sales",
                table: "SalesOrderAmendmentDiscount",
                column: "DiscountMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentDiscount_SalesOrderAmendmentHeaderId",
                schema: "Sales",
                table: "SalesOrderAmendmentDiscount",
                column: "SalesOrderAmendmentHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderAmendmentDiscount_SalesOrderDiscountId",
                schema: "Sales",
                table: "SalesOrderAmendmentDiscount",
                column: "SalesOrderDiscountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderAmendmentDiscount",
                schema: "Sales");

            migrationBuilder.DropColumn(
                name: "AgentCommissionId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "AgentCommissionSlabId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "AgentPaymentTermsId",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "CommissionValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "MdDiscountValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "TotalDiscountValue",
                schema: "Sales",
                table: "SalesOrderAmendmentHeader");

            migrationBuilder.DropColumn(
                name: "AgentCommissionPercentage",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");
        }
    }
}
