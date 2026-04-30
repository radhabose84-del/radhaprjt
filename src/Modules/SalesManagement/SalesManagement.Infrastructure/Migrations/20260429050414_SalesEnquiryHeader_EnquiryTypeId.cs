using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesEnquiryHeader_EnquiryTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnquiryTypeId",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows to ENQ_DOMESTIC before the FK is added.
            // Subquery resolves the Id dynamically; runs in the same transaction
            // as the schema change (atomic with rollback safety).
            migrationBuilder.Sql(@"
                UPDATE Sales.SalesEnquiryHeader
                SET    EnquiryTypeId = (
                    SELECT mm.Id
                    FROM   Sales.MiscMaster mm
                    INNER  JOIN Sales.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
                    WHERE  mt.MiscTypeCode = 'ENQ_TYPE'
                      AND  mm.Code         = 'ENQ_DOMESTIC'
                      AND  mm.IsDeleted    = 0
                      AND  mt.IsDeleted    = 0
                )
                WHERE  EnquiryTypeId = 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_SalesEnquiryHeader_EnquiryTypeId",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                column: "EnquiryTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesEnquiryHeader_MiscMaster_EnquiryTypeId",
                schema: "Sales",
                table: "SalesEnquiryHeader",
                column: "EnquiryTypeId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesEnquiryHeader_MiscMaster_EnquiryTypeId",
                schema: "Sales",
                table: "SalesEnquiryHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesEnquiryHeader_EnquiryTypeId",
                schema: "Sales",
                table: "SalesEnquiryHeader");

            migrationBuilder.DropColumn(
                name: "EnquiryTypeId",
                schema: "Sales",
                table: "SalesEnquiryHeader");
        }
    }
}
