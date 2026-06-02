using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TnCTemplate_ModuleAndTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TnCTemplateApplicability_MiscMaster_ApplicabilityId",
                schema: "Purchase",
                table: "TnCTemplateApplicability");

            migrationBuilder.DropForeignKey(
                name: "FK_TnCTemplateMaster_MiscMaster_TemplateTypeId",
                schema: "Purchase",
                table: "TnCTemplateMaster");

            migrationBuilder.DropIndex(
                name: "IX_TnCTemplateApplicability_ApplicabilityId",
                schema: "Purchase",
                table: "TnCTemplateApplicability");

            migrationBuilder.RenameColumn(
                name: "TemplateTypeId",
                schema: "Purchase",
                table: "TnCTemplateMaster",
                newName: "ModuleId");

            migrationBuilder.RenameIndex(
                name: "UX_TnC_Type_Name",
                schema: "Purchase",
                table: "TnCTemplateMaster",
                newName: "UX_TnC_Module_Name");

            migrationBuilder.RenameColumn(
                name: "ApplicabilityId",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                newName: "TransactionTypeId");

            migrationBuilder.RenameIndex(
                name: "UX_TnC_Template_App",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                newName: "UX_TnC_Template_TxnType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModuleId",
                schema: "Purchase",
                table: "TnCTemplateMaster",
                newName: "TemplateTypeId");

            migrationBuilder.RenameIndex(
                name: "UX_TnC_Module_Name",
                schema: "Purchase",
                table: "TnCTemplateMaster",
                newName: "UX_TnC_Type_Name");

            migrationBuilder.RenameColumn(
                name: "TransactionTypeId",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                newName: "ApplicabilityId");

            migrationBuilder.RenameIndex(
                name: "UX_TnC_Template_TxnType",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                newName: "UX_TnC_Template_App");

            migrationBuilder.CreateIndex(
                name: "IX_TnCTemplateApplicability_ApplicabilityId",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                column: "ApplicabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TnCTemplateApplicability_MiscMaster_ApplicabilityId",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                column: "ApplicabilityId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TnCTemplateMaster_MiscMaster_TemplateTypeId",
                schema: "Purchase",
                table: "TnCTemplateMaster",
                column: "TemplateTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
