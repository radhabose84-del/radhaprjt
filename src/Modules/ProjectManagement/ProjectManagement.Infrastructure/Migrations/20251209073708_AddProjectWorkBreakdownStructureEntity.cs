using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectWorkBreakdownStructureEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectWorkBreakdownStructure",
                schema: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ParentWorkBreakdownStructureId = table.Column<int>(type: "int", nullable: true),
                    WorkBreakdownStructureName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkBreakdownStructureDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DurationInDays = table.Column<int>(type: "int", nullable: true),
                    ResponsibleDepartmentId = table.Column<int>(type: "int", nullable: false),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    PlannedBudgetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    IsMilestone = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MilestoneDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    BudgetYearId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectWorkBreakdownStructure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectWorkBreakdownStructure_ProjectMaster_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "Project",
                        principalTable: "ProjectMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectWorkBreakdownStructure_ProjectWorkBreakdownStructure_ParentWorkBreakdownStructureId",
                        column: x => x.ParentWorkBreakdownStructureId,
                        principalSchema: "Project",
                        principalTable: "ProjectWorkBreakdownStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkBreakdownStructure_ParentWorkBreakdownStructureId",
                schema: "Project",
                table: "ProjectWorkBreakdownStructure",
                column: "ParentWorkBreakdownStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkBreakdownStructure_ProjectId",
                schema: "Project",
                table: "ProjectWorkBreakdownStructure",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkBreakdownStructure_ProjectId_WorkBreakdownStructureName",
                schema: "Project",
                table: "ProjectWorkBreakdownStructure",
                columns: new[] { "ProjectId", "WorkBreakdownStructureName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectWorkBreakdownStructure",
                schema: "Project");
        }
    }
}
