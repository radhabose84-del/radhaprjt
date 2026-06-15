using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleIIIStructureMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleIIIStructure",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    StructureStatusId = table.Column<int>(type: "int", nullable: false),
                    TextileSplitEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_ScheduleIIIStructure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIStructure_MiscMaster_StructureStatusId",
                        column: x => x.StructureStatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleIIISection",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(150)", nullable: false),
                    StatementTypeId = table.Column<int>(type: "int", nullable: false),
                    NatureId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_ScheduleIIISection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISection_MiscMaster_NatureId",
                        column: x => x.NatureId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISection_MiscMaster_StatementTypeId",
                        column: x => x.StatementTypeId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISection_ScheduleIIIStructure_StructureId",
                        column: x => x.StructureId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIIStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleIIISubTotal",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    SubTotalName = table.Column<string>(type: "varchar(120)", nullable: false),
                    FormulaExpression = table.Column<string>(type: "varchar(500)", nullable: false),
                    IncludeOtherIncome = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_ScheduleIIISubTotal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISubTotal_ScheduleIIIStructure_StructureId",
                        column: x => x.StructureId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIIStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleIIILineItem",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    ParentLineId = table.Column<int>(type: "int", nullable: true),
                    LineCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    LineName = table.Column<string>(type: "varchar(200)", nullable: false),
                    SubClassification = table.Column<string>(type: "varchar(120)", nullable: true),
                    NoteReference = table.Column<string>(type: "varchar(30)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsSplitLine = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_ScheduleIIILineItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIILineItem_ScheduleIIILineItem_ParentLineId",
                        column: x => x.ParentLineId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIILineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIILineItem_ScheduleIIISection_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIISection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIILineItem_ScheduleIIIStructure_StructureId",
                        column: x => x.StructureId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIIStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleIIISubTotalFormula",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubTotalId = table.Column<int>(type: "int", nullable: false),
                    OperandTypeId = table.Column<int>(type: "int", nullable: false),
                    OperandRefId = table.Column<int>(type: "int", nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_ScheduleIIISubTotalFormula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISubTotalFormula_MiscMaster_OperandTypeId",
                        column: x => x.OperandTypeId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISubTotalFormula_MiscMaster_OperatorId",
                        column: x => x.OperatorId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISubTotalFormula_ScheduleIIISubTotal_SubTotalId",
                        column: x => x.SubTotalId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIISubTotal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_ParentLineId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "ParentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_SectionId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_StructureId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISection_NatureId",
                schema: "Finance",
                table: "ScheduleIIISection",
                column: "NatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISection_StatementTypeId",
                schema: "Finance",
                table: "ScheduleIIISection",
                column: "StatementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISection_StructureId",
                schema: "Finance",
                table: "ScheduleIIISection",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_CompanyId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                columns: new[] { "CompanyId", "DivisionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_StructureStatusId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                column: "StructureStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotal_StructureId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotalFormula_OperandTypeId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                column: "OperandTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotalFormula_OperatorId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotalFormula_SubTotalId",
                schema: "Finance",
                table: "ScheduleIIISubTotalFormula",
                column: "SubTotalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleIIILineItem",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "ScheduleIIISubTotalFormula",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "ScheduleIIISection",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "ScheduleIIISubTotal",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "ScheduleIIIStructure",
                schema: "Finance");
        }
    }
}
