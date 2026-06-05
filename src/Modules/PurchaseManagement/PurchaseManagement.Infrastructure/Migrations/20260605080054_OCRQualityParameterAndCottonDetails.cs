using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OCRQualityParameterAndCottonDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CottonPassedBy",
                schema: "Purchase",
                table: "OCREntry",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GstPercentage",
                schema: "Purchase",
                table: "OCREntry",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LorryFreightId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MillSampleNo",
                schema: "Purchase",
                table: "OCREntry",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentModeId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QualityTemplateId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RateUnitId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                schema: "Purchase",
                table: "OCREntry",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransitInsuranceId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeighmentId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OCRQualityParameter",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcrId = table.Column<int>(type: "int", nullable: false),
                    QualityTemplateId = table.Column<int>(type: "int", nullable: false),
                    ParamId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "varchar(200)", nullable: true),
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
                    table.PrimaryKey("PK_OCRQualityParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OCRQualityParameter_OCREntry_OcrId",
                        column: x => x.OcrId,
                        principalSchema: "Purchase",
                        principalTable: "OCREntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_LorryFreightId",
                schema: "Purchase",
                table: "OCREntry",
                column: "LorryFreightId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_PaymentModeId",
                schema: "Purchase",
                table: "OCREntry",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_RateUnitId",
                schema: "Purchase",
                table: "OCREntry",
                column: "RateUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_TransitInsuranceId",
                schema: "Purchase",
                table: "OCREntry",
                column: "TransitInsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_WeighmentId",
                schema: "Purchase",
                table: "OCREntry",
                column: "WeighmentId");

            migrationBuilder.CreateIndex(
                name: "IX_OCRQualityParameter_OcrId",
                schema: "Purchase",
                table: "OCRQualityParameter",
                column: "OcrId");

            migrationBuilder.CreateIndex(
                name: "IX_OCRQualityParameter_QualityTemplateId",
                schema: "Purchase",
                table: "OCRQualityParameter",
                column: "QualityTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_LorryFreightId",
                schema: "Purchase",
                table: "OCREntry",
                column: "LorryFreightId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_PaymentModeId",
                schema: "Purchase",
                table: "OCREntry",
                column: "PaymentModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_RateUnitId",
                schema: "Purchase",
                table: "OCREntry",
                column: "RateUnitId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_TransitInsuranceId",
                schema: "Purchase",
                table: "OCREntry",
                column: "TransitInsuranceId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_WeighmentId",
                schema: "Purchase",
                table: "OCREntry",
                column: "WeighmentId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_LorryFreightId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_PaymentModeId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_RateUnitId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_TransitInsuranceId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_WeighmentId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropTable(
                name: "OCRQualityParameter",
                schema: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_LorryFreightId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_PaymentModeId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_RateUnitId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_TransitInsuranceId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_WeighmentId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "CottonPassedBy",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "GstPercentage",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "LorryFreightId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "MillSampleNo",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "PaymentModeId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "QualityTemplateId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "RateUnitId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "Remarks",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "TransitInsuranceId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "WeighmentId",
                schema: "Purchase",
                table: "OCREntry");
        }
    }
}
