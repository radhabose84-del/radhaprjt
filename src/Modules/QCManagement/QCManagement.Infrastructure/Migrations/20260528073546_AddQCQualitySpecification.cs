using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QCManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQCQualitySpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualitySpecification",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecificationCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    SpecificationName = table.Column<string>(type: "varchar(100)", nullable: false),
                    QualityTemplateId = table.Column<int>(type: "int", nullable: false),
                    ApplicableLevelId = table.Column<int>(type: "int", nullable: false),
                    ItemCategoryId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_QualitySpecification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualitySpecification_MiscMaster_ApplicableLevelId",
                        column: x => x.ApplicableLevelId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualitySpecification_QualityTemplate_QualityTemplateId",
                        column: x => x.QualityTemplateId,
                        principalSchema: "QC",
                        principalTable: "QualityTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualitySpecificationParameter",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QualitySpecificationId = table.Column<int>(type: "int", nullable: false),
                    QualityParameterId = table.Column<int>(type: "int", nullable: false),
                    ValidationTypeId = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    ExpectedValue = table.Column<string>(type: "varchar(250)", nullable: true),
                    AllowedValues = table.Column<string>(type: "varchar(2000)", nullable: true),
                    SeverityId = table.Column<int>(type: "int", nullable: true),
                    FailureActionId = table.Column<int>(type: "int", nullable: true),
                    IsSamplingRequired = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_QualitySpecificationParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualitySpecificationParameter_MiscMaster_FailureActionId",
                        column: x => x.FailureActionId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualitySpecificationParameter_MiscMaster_SeverityId",
                        column: x => x.SeverityId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualitySpecificationParameter_MiscMaster_ValidationTypeId",
                        column: x => x.ValidationTypeId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualitySpecificationParameter_QualityParameter_QualityParameterId",
                        column: x => x.QualityParameterId,
                        principalSchema: "QC",
                        principalTable: "QualityParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualitySpecificationParameter_QualitySpecification_QualitySpecificationId",
                        column: x => x.QualitySpecificationId,
                        principalSchema: "QC",
                        principalTable: "QualitySpecification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_ApplicableLevelId",
                schema: "QC",
                table: "QualitySpecification",
                column: "ApplicableLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_EffectiveFrom",
                schema: "QC",
                table: "QualitySpecification",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_ItemCategoryId",
                schema: "QC",
                table: "QualitySpecification",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_ItemId",
                schema: "QC",
                table: "QualitySpecification",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_QualityTemplateId",
                schema: "QC",
                table: "QualitySpecification",
                column: "QualityTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_SpecificationCode",
                schema: "QC",
                table: "QualitySpecification",
                column: "SpecificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_SpecificationName",
                schema: "QC",
                table: "QualitySpecification",
                column: "SpecificationName");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecificationParameter_FailureActionId",
                schema: "QC",
                table: "QualitySpecificationParameter",
                column: "FailureActionId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecificationParameter_QualityParameterId",
                schema: "QC",
                table: "QualitySpecificationParameter",
                column: "QualityParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecificationParameter_QualitySpecificationId",
                schema: "QC",
                table: "QualitySpecificationParameter",
                column: "QualitySpecificationId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecificationParameter_QualitySpecificationId_QualityParameterId",
                schema: "QC",
                table: "QualitySpecificationParameter",
                columns: new[] { "QualitySpecificationId", "QualityParameterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecificationParameter_SeverityId",
                schema: "QC",
                table: "QualitySpecificationParameter",
                column: "SeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecificationParameter_ValidationTypeId",
                schema: "QC",
                table: "QualitySpecificationParameter",
                column: "ValidationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualitySpecificationParameter",
                schema: "QC");

            migrationBuilder.DropTable(
                name: "QualitySpecification",
                schema: "QC");
        }
    }
}
