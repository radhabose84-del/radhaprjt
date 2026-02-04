using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BudgetMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Budget");

            migrationBuilder.CreateTable(
                name: "BudgetMaster",
                schema: "Budget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    BudgetGroupId = table.Column<int>(type: "int", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    YearBudgetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Is_MRApplicable = table.Column<bool>(type: "bit", nullable: true),
                    Is_POApplicable = table.Column<bool>(type: "bit", nullable: true),
                    Is_ServiceApplicable = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_BudgetMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetDetail",
                schema: "Budget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetId = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_BudgetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetDetail_BudgetMaster_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "Budget",
                        principalTable: "BudgetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetLog",
                schema: "Budget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetDetailId = table.Column<int>(type: "int", nullable: false),
                    ActionTypeId = table.Column<int>(type: "int", nullable: false),
                    OldBudgetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewBudgetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(1000)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetLog_BudgetDetail_BudgetDetailId",
                        column: x => x.BudgetDetailId,
                        principalSchema: "Budget",
                        principalTable: "BudgetDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategory_RootCategoryId",
                schema: "Inventory",
                table: "ItemCategory",
                column: "RootCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetDetail_BudgetId",
                schema: "Budget",
                table: "BudgetDetail",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLog_BudgetDetailId",
                schema: "Budget",
                table: "BudgetLog",
                column: "BudgetDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategory_ItemCategory_RootCategoryId",
                schema: "Inventory",
                table: "ItemCategory",
                column: "RootCategoryId",
                principalSchema: "Inventory",
                principalTable: "ItemCategory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategory_ItemCategory_RootCategoryId",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.DropTable(
                name: "BudgetLog",
                schema: "Budget");

            migrationBuilder.DropTable(
                name: "BudgetDetail",
                schema: "Budget");

            migrationBuilder.DropTable(
                name: "BudgetMaster",
                schema: "Budget");

            migrationBuilder.DropIndex(
                name: "IX_ItemCategory_RootCategoryId",
                schema: "Inventory",
                table: "ItemCategory");
        }
    }
}
