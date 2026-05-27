using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QCManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQCQualityTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualityTemplate",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    TemplateName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_QualityTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityTemplateParameter",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualityTemplateId = table.Column<int>(type: "int", nullable: false),
                    QualityParameterId = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    InspectionMethodId = table.Column<int>(type: "int", nullable: true),
                    SampleSize = table.Column<int>(type: "int", nullable: true),
                    SampleUomId = table.Column<int>(type: "int", nullable: true),
                    IsGradeApplicable = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_QualityTemplateParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityTemplateParameter_MiscMaster_InspectionMethodId",
                        column: x => x.InspectionMethodId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityTemplateParameter_QualityParameter_QualityParameterId",
                        column: x => x.QualityParameterId,
                        principalSchema: "QC",
                        principalTable: "QualityParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityTemplateParameter_QualityTemplate_QualityTemplateId",
                        column: x => x.QualityTemplateId,
                        principalSchema: "QC",
                        principalTable: "QualityTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplate_TemplateCode",
                schema: "QC",
                table: "QualityTemplate",
                column: "TemplateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplate_TemplateName",
                schema: "QC",
                table: "QualityTemplate",
                column: "TemplateName");

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplateParameter_InspectionMethodId",
                schema: "QC",
                table: "QualityTemplateParameter",
                column: "InspectionMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplateParameter_QualityParameterId",
                schema: "QC",
                table: "QualityTemplateParameter",
                column: "QualityParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplateParameter_QualityTemplateId",
                schema: "QC",
                table: "QualityTemplateParameter",
                column: "QualityTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplateParameter_QualityTemplateId_QualityParameterId",
                schema: "QC",
                table: "QualityTemplateParameter",
                columns: new[] { "QualityTemplateId", "QualityParameterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityTemplateParameter_SampleUomId",
                schema: "QC",
                table: "QualityTemplateParameter",
                column: "SampleUomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityTemplateParameter",
                schema: "QC");

            migrationBuilder.DropTable(
                name: "QualityTemplate",
                schema: "QC");
        }
    }
}
