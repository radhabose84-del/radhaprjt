using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CostCentreMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCentre",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CostCentreCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    CostCentreName = table.Column<string>(type: "varchar(100)", nullable: false),
                    CentreLevelId = table.Column<int>(type: "int", nullable: false),
                    ParentCostCentreId = table.Column<int>(type: "int", nullable: true),
                    DepartmentGroupId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    ResponsibleManagerId = table.Column<int>(type: "int", nullable: true),
                    EffectiveFromDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EffectiveToDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_CostCentre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostCentre_CostCentre_ParentCostCentreId",
                        column: x => x.ParentCostCentreId,
                        principalSchema: "Finance",
                        principalTable: "CostCentre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CostCentre_MiscMaster_CentreLevelId",
                        column: x => x.CentreLevelId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostCentre_CentreLevelId",
                schema: "Finance",
                table: "CostCentre",
                column: "CentreLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCentre_DepartmentGroupId",
                schema: "Finance",
                table: "CostCentre",
                column: "DepartmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCentre_DepartmentId",
                schema: "Finance",
                table: "CostCentre",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCentre_ParentCostCentreId",
                schema: "Finance",
                table: "CostCentre",
                column: "ParentCostCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCentre_UnitId",
                schema: "Finance",
                table: "CostCentre",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCentre_UnitId_CostCentreCode",
                schema: "Finance",
                table: "CostCentre",
                columns: new[] { "UnitId", "CostCentreCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostCentre",
                schema: "Finance");
        }
    }
}
