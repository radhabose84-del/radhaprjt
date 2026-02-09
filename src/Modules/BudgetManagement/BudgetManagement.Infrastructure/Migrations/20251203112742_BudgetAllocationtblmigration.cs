using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BudgetAllocationtblmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetAllocation",
                schema: "Budget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    RequestById = table.Column<int>(type: "int", nullable: false),
                    RequestMonthId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    RequestTypeId = table.Column<int>(type: "int", nullable: true),
                    BudgetGroupId = table.Column<int>(type: "int", nullable: false),
                    BudgetSubGroupId = table.Column<int>(type: "int", nullable: true),
                    AllocationTypeId = table.Column<int>(type: "int", nullable: false),
                    SpindleCount = table.Column<int>(type: "int", nullable: true),
                    RatePerSpindle = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetAllocation_BudgetGroup_BudgetGroupId",
                        column: x => x.BudgetGroupId,
                        principalSchema: "Budget",
                        principalTable: "BudgetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocation_BudgetRequest_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalSchema: "Budget",
                        principalTable: "BudgetRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocation_MiscMaster_AllocationTypeId",
                        column: x => x.AllocationTypeId,
                        principalSchema: "Budget",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocation_MiscMaster_RequestById",
                        column: x => x.RequestById,
                        principalSchema: "Budget",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetAllocation_MiscMaster_RequestMonthId",
                        column: x => x.RequestMonthId,
                        principalSchema: "Budget",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocation_AllocationTypeId",
                schema: "Budget",
                table: "BudgetAllocation",
                column: "AllocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocation_BudgetGroupId",
                schema: "Budget",
                table: "BudgetAllocation",
                column: "BudgetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocation_RequestById",
                schema: "Budget",
                table: "BudgetAllocation",
                column: "RequestById");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocation_RequestMonthId",
                schema: "Budget",
                table: "BudgetAllocation",
                column: "RequestMonthId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetAllocation_RequestTypeId",
                schema: "Budget",
                table: "BudgetAllocation",
                column: "RequestTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetAllocation",
                schema: "Budget");
        }
    }
}
