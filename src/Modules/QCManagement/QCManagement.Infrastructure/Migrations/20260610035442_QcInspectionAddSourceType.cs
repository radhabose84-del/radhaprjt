using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QCManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QcInspectionAddSourceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QcInspectionHdr_GrnDetailId",
                schema: "QC",
                table: "QcInspectionHdr");

            migrationBuilder.DropIndex(
                name: "IX_QcInspectionHdr_GrnHeaderId",
                schema: "QC",
                table: "QcInspectionHdr");

            // Preserve data with the CORRECT rename mapping:
            //   GrnHeaderId -> SourceHeaderId, GrnDetailId -> SourceDetailId
            migrationBuilder.RenameColumn(
                name: "GrnHeaderId",
                schema: "QC",
                table: "QcInspectionHdr",
                newName: "SourceHeaderId");

            migrationBuilder.RenameColumn(
                name: "GrnDetailId",
                schema: "QC",
                table: "QcInspectionHdr",
                newName: "SourceDetailId");

            // New source-type discriminator column (backfilled below before the FK is added).
            migrationBuilder.AddColumn<int>(
                name: "SourceTypeId",
                schema: "QC",
                table: "QcInspectionHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows to GRN (requires QP_SOURCE_TYPE 'GRN' to be seeded first).
            migrationBuilder.Sql(@"
                UPDATE QC.QcInspectionHdr
                SET SourceTypeId = (
                    SELECT TOP 1 mm.Id
                    FROM QC.MiscMaster mm
                    INNER JOIN QC.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                    WHERE mt.MiscTypeCode = 'QP_SOURCE_TYPE' AND mm.Code = 'GRN'
                      AND mm.IsActive = 1 AND mm.IsDeleted = 0)
                WHERE SourceTypeId = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_SourceTypeId_SourceDetailId",
                schema: "QC",
                table: "QcInspectionHdr",
                columns: new[] { "SourceTypeId", "SourceDetailId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QcInspectionHdr_SourceTypeId_SourceHeaderId",
                schema: "QC",
                table: "QcInspectionHdr",
                columns: new[] { "SourceTypeId", "SourceHeaderId" });

            migrationBuilder.AddForeignKey(
                name: "FK_QcInspectionHdr_MiscMaster_SourceTypeId",
                schema: "QC",
                table: "QcInspectionHdr",
                column: "SourceTypeId",
                principalSchema: "QC",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QcInspectionHdr_MiscMaster_SourceTypeId",
                schema: "QC",
                table: "QcInspectionHdr");

            migrationBuilder.DropIndex(
                name: "IX_QcInspectionHdr_SourceTypeId_SourceDetailId",
                schema: "QC",
                table: "QcInspectionHdr");

            migrationBuilder.DropIndex(
                name: "IX_QcInspectionHdr_SourceTypeId_SourceHeaderId",
                schema: "QC",
                table: "QcInspectionHdr");

            migrationBuilder.DropColumn(
                name: "SourceTypeId",
                schema: "QC",
                table: "QcInspectionHdr");

            migrationBuilder.RenameColumn(
                name: "SourceHeaderId",
                schema: "QC",
                table: "QcInspectionHdr",
                newName: "GrnHeaderId");

            migrationBuilder.RenameColumn(
                name: "SourceDetailId",
                schema: "QC",
                table: "QcInspectionHdr",
                newName: "GrnDetailId");

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
        }
    }
}
