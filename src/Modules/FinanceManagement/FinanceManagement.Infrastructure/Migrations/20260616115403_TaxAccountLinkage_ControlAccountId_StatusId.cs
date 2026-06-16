using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaxAccountLinkage_ControlAccountId_StatusId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropIndex(
                name: "IX_TaxCodeMaster_TaxType",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TCM_Component",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TCM_Direction",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TCM_ParentLink",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TCM_TaxType",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TAL_Status",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "Direction",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropColumn(
                name: "TaxComponent",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropColumn(
                name: "TaxType",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.RenameColumn(
                name: "ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "ControlAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_TaxAccountLinkage_ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "IX_TaxAccountLinkage_ControlAccountId");

            migrationBuilder.AddColumn<int>(
                name: "DirectionId",
                schema: "Finance",
                table: "TaxCodeMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxComponentId",
                schema: "Finance",
                table: "TaxCodeMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxTypeId",
                schema: "Finance",
                table: "TaxCodeMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeMaster_DirectionId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeMaster_TaxComponentId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "TaxComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeMaster_TaxTypeId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "TaxTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxAccountLinkage_StatusId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage",
                columns: new[] { "CompanyId", "GlAccountId" },
                unique: true,
                filter: "[EffectiveTo] IS NULL AND [IsActivated] = 1 AND [IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_ControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "ControlAccountId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_StatusId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "StatusId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxCodeMaster_MiscMaster_DirectionId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "DirectionId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxCodeMaster_MiscMaster_TaxComponentId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "TaxComponentId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxCodeMaster_MiscMaster_TaxTypeId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "TaxTypeId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_ControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_StatusId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxCodeMaster_MiscMaster_DirectionId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxCodeMaster_MiscMaster_TaxComponentId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxCodeMaster_MiscMaster_TaxTypeId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropIndex(
                name: "IX_TaxCodeMaster_DirectionId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropIndex(
                name: "IX_TaxCodeMaster_TaxComponentId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropIndex(
                name: "IX_TaxCodeMaster_TaxTypeId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropIndex(
                name: "IX_TaxAccountLinkage_StatusId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "DirectionId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropColumn(
                name: "TaxComponentId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropColumn(
                name: "TaxTypeId",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.RenameColumn(
                name: "ControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "ControlAccountTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_TaxAccountLinkage_ControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "IX_TaxAccountLinkage_ControlAccountTypeId");

            migrationBuilder.AddColumn<string>(
                name: "Direction",
                schema: "Finance",
                table: "TaxCodeMaster",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxComponent",
                schema: "Finance",
                table: "TaxCodeMaster",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "COMBINED");

            migrationBuilder.AddColumn<string>(
                name: "TaxType",
                schema: "Finance",
                table: "TaxCodeMaster",
                type: "varchar(15)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "varchar(15)",
                nullable: false,
                defaultValue: "PENDING");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeMaster_TaxType",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "TaxType");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TCM_Component",
                schema: "Finance",
                table: "TaxCodeMaster",
                sql: "[TaxComponent] IN ('COMBINED','CGST','SGST','IGST','CESS','NA')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TCM_Direction",
                schema: "Finance",
                table: "TaxCodeMaster",
                sql: "[Direction] IS NULL OR [Direction] IN ('INPUT','OUTPUT')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TCM_ParentLink",
                schema: "Finance",
                table: "TaxCodeMaster",
                sql: "([TaxComponent] IN ('CGST','SGST','IGST','CESS') AND [ParentTaxCodeId] IS NOT NULL) OR ([TaxComponent] IN ('COMBINED','NA') AND [ParentTaxCodeId] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TCM_TaxType",
                schema: "Finance",
                table: "TaxCodeMaster",
                sql: "[TaxType] IN ('GST_IN','GST_OUT','IGST','TDS','CUSTOMS')");

            migrationBuilder.CreateIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage",
                columns: new[] { "CompanyId", "GlAccountId" },
                unique: true,
                filter: "[EffectiveTo] IS NULL AND [ApprovalStatus] = 'APPROVED' AND [IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TAL_Status",
                schema: "Finance",
                table: "TaxAccountLinkage",
                sql: "[ApprovalStatus] IN ('PENDING','APPROVED','REJECTED')");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_ControlAccountTypeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "ControlAccountTypeId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
