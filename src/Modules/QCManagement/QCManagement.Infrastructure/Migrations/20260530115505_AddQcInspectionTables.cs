using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QCManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQcInspectionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLog",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityName = table.Column<string>(type: "varchar(60)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "varchar(40)", nullable: false),
                    PropertyName = table.Column<string>(type: "varchar(200)", nullable: false),
                    OldValue = table.Column<string>(type: "varchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "varchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    Scope = table.Column<string>(type: "varchar(40)", nullable: true),
                    ScopeKey = table.Column<string>(type: "varchar(120)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QcInspectionHdr",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QcInspectionNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    InspectionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GrnHeaderId = table.Column<int>(type: "int", nullable: false),
                    GrnDetailId = table.Column<int>(type: "int", nullable: false),
                    QualitySpecificationId = table.Column<int>(type: "int", nullable: false),
                    QualitySpecificationCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    QualityTemplateId = table.Column<int>(type: "int", nullable: false),
                    QualityTemplateCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    QcTypeId = table.Column<int>(type: "int", nullable: false),
                    InspectorUserId = table.Column<int>(type: "int", nullable: false),
                    InspectorName = table.Column<string>(type: "varchar(100)", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ReceivedUomId = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    LotNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    QcStatusId = table.Column<int>(type: "int", nullable: true),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    DispositionRemarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    DispositionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DispositionByUserId = table.Column<int>(type: "int", nullable: true),
                    DispositionByName = table.Column<string>(type: "varchar(100)", nullable: true),
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
                    table.PrimaryKey("PK_QcInspectionHdr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcInspectionHdr_MiscMaster_QcStatusId",
                        column: x => x.QcStatusId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QcInspectionDtl",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QcInspectionHdrId = table.Column<int>(type: "int", nullable: false),
                    QualitySpecificationParameterId = table.Column<int>(type: "int", nullable: false),
                    QualityParameterId = table.Column<int>(type: "int", nullable: false),
                    ParameterCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ParameterName = table.Column<string>(type: "varchar(100)", nullable: false),
                    DataTypeId = table.Column<int>(type: "int", nullable: false),
                    ValidationTypeId = table.Column<int>(type: "int", nullable: false),
                    ValidationTypeCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    UomCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ExpectedValue = table.Column<string>(type: "varchar(200)", nullable: true),
                    AllowedValues = table.Column<string>(type: "varchar(2000)", nullable: true),
                    SeverityId = table.Column<int>(type: "int", nullable: false),
                    SeverityCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    FailureActionId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ActualValue = table.Column<string>(type: "varchar(200)", nullable: true),
                    InspectionResult = table.Column<string>(type: "varchar(10)", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_QcInspectionDtl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcInspectionDtl_QcInspectionHdr_QcInspectionHdrId",
                        column: x => x.QcInspectionHdrId,
                        principalSchema: "QC",
                        principalTable: "QcInspectionHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_Entity_CreatedDate",
                schema: "QC",
                table: "ActivityLog",
                columns: new[] { "EntityName", "EntityId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionDtl_QcInspectionHdrId_SortOrder",
                schema: "QC",
                table: "QcInspectionDtl",
                columns: new[] { "QcInspectionHdrId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_BatchNumber",
                schema: "QC",
                table: "QcInspectionHdr",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_GrnDetailId",
                schema: "QC",
                table: "QcInspectionHdr",
                column: "GrnDetailId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_GrnHeaderId",
                schema: "QC",
                table: "QcInspectionHdr",
                column: "GrnHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_InspectionDate_QcStatusId",
                schema: "QC",
                table: "QcInspectionHdr",
                columns: new[] { "InspectionDate", "QcStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_QcInspectionNo",
                schema: "QC",
                table: "QcInspectionHdr",
                column: "QcInspectionNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_QcStatusId",
                schema: "QC",
                table: "QcInspectionHdr",
                column: "QcStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLog",
                schema: "QC");

            migrationBuilder.DropTable(
                name: "QcInspectionDtl",
                schema: "QC");

            migrationBuilder.DropTable(
                name: "QcInspectionHdr",
                schema: "QC");
        }
    }
}
