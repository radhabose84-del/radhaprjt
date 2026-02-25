using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_MiscMaster_MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropIndex(
                name: "IX_BudgetRequest_MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.CreateTable(
                name: "BudgetGroup",
                schema: "Budget",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    ParentBudgetGroupId = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    AllocationRuleId = table.Column<int>(type: "int", nullable: true),
                    AllocatedPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AllocatedSpindleCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsParent = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetGroup_BudgetGroup_ParentBudgetGroupId",
                        column: x => x.ParentBudgetGroupId,
                        principalSchema: "Budget",
                        principalTable: "BudgetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetGroup_MiscMaster_AllocationRuleId",
                        column: x => x.AllocationRuleId,
                        principalSchema: "Budget",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetGroup_AllocationRuleId",
                schema: "Budget",
                table: "BudgetGroup",
                column: "AllocationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetGroup_ParentBudgetGroupId",
                schema: "Budget",
                table: "BudgetGroup",
                column: "ParentBudgetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetGroup_UnitId_DepartmentId_Name",
                schema: "Budget",
                table: "BudgetGroup",
                columns: new[] { "UnitId", "DepartmentId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestTypeId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropTable(
                name: "BudgetGroup",
                schema: "Budget");

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRequest_MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_MiscMasterId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "MiscMasterId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_MiscMaster_RequestTypeId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "RequestTypeId",
                principalSchema: "Budget",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
