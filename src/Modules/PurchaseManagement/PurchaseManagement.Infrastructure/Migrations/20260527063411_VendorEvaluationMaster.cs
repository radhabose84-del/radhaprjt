using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VendorEvaluationMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryScoreRule",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", nullable: false),
                    DelayDaysFrom = table.Column<int>(type: "int", nullable: false),
                    DelayDaysTo = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryScoreRule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorEvaluationCriteria",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CriteriaCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    CriteriaName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    WeightagePercent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    ScoringMethodId = table.Column<int>(type: "int", nullable: false),
                    MinimumScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    RatingImpactId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorEvaluationCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorEvaluationCriteria_MiscMaster_RatingImpactId",
                        column: x => x.RatingImpactId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorEvaluationCriteria_MiscMaster_ScoringMethodId",
                        column: x => x.ScoringMethodId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorRatingGrade",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    GradeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MinScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    MaxScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    ActionDescription = table.Column<string>(type: "varchar(200)", nullable: true),
                    ActionTypeId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorRatingGrade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorRatingGrade_MiscMaster_ActionTypeId",
                        column: x => x.ActionTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorEvaluationHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    EvaluationMonth = table.Column<int>(type: "int", nullable: false),
                    EvaluationYear = table.Column<int>(type: "int", nullable: false),
                    EvaluationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalWeightedScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorEvaluationHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorEvaluationHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorEvaluationHeader_VendorRatingGrade_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "Purchase",
                        principalTable: "VendorRatingGrade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorEvaluationDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorEvaluationHeaderId = table.Column<int>(type: "int", nullable: false),
                    CriteriaId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    WeightagePercent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    WeightedScore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    ScoringMethod = table.Column<string>(type: "varchar(20)", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorEvaluationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorEvaluationDetail_VendorEvaluationCriteria_CriteriaId",
                        column: x => x.CriteriaId,
                        principalSchema: "Purchase",
                        principalTable: "VendorEvaluationCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendorEvaluationDetail_VendorEvaluationHeader_VendorEvaluationHeaderId",
                        column: x => x.VendorEvaluationHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "VendorEvaluationHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryScoreRule_RuleCode",
                schema: "Purchase",
                table: "DeliveryScoreRule",
                column: "RuleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationCriteria_CriteriaCode",
                schema: "Purchase",
                table: "VendorEvaluationCriteria",
                column: "CriteriaCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationCriteria_RatingImpactId",
                schema: "Purchase",
                table: "VendorEvaluationCriteria",
                column: "RatingImpactId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationCriteria_ScoringMethodId",
                schema: "Purchase",
                table: "VendorEvaluationCriteria",
                column: "ScoringMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationDetail_CriteriaId",
                schema: "Purchase",
                table: "VendorEvaluationDetail",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationDetail_VendorEvaluationHeaderId",
                schema: "Purchase",
                table: "VendorEvaluationDetail",
                column: "VendorEvaluationHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationHeader_EvaluationCode",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                column: "EvaluationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationHeader_GradeId",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationHeader_StatusId",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationHeader_VendorId",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorEvaluationHeader_VendorId_EvaluationMonth_EvaluationYear",
                schema: "Purchase",
                table: "VendorEvaluationHeader",
                columns: new[] { "VendorId", "EvaluationMonth", "EvaluationYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorRatingGrade_ActionTypeId",
                schema: "Purchase",
                table: "VendorRatingGrade",
                column: "ActionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorRatingGrade_GradeCode",
                schema: "Purchase",
                table: "VendorRatingGrade",
                column: "GradeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryScoreRule",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "VendorEvaluationDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "VendorEvaluationCriteria",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "VendorEvaluationHeader",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "VendorRatingGrade",
                schema: "Purchase");
        }
    }
}
