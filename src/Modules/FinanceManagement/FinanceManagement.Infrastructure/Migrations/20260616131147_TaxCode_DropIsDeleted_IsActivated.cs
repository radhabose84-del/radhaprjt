using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaxCode_DropIsDeleted_IsActivated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_TaxCodeRateVersion_OpenPerCode",
                schema: "Finance",
                table: "TaxCodeRateVersion");

            migrationBuilder.DropIndex(
                name: "UX_TaxCodeMaster_Company_Code",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Finance",
                table: "TaxCodeRateVersion");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropColumn(
                name: "IsActivated",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.AddColumn<string>(
                name: "ChangeReason",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_TaxCodeRateVersion_OpenPerCode",
                schema: "Finance",
                table: "TaxCodeRateVersion",
                column: "TaxCodeId",
                unique: true,
                filter: "[EffectiveTo] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_TaxCodeMaster_Company_Code",
                schema: "Finance",
                table: "TaxCodeMaster",
                columns: new[] { "CompanyId", "TaxCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage",
                columns: new[] { "CompanyId", "GlAccountId" },
                unique: true,
                filter: "[EffectiveTo] IS NULL AND [IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_TaxCodeRateVersion_OpenPerCode",
                schema: "Finance",
                table: "TaxCodeRateVersion");

            migrationBuilder.DropIndex(
                name: "UX_TaxCodeMaster_Company_Code",
                schema: "Finance",
                table: "TaxCodeMaster");

            migrationBuilder.DropIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "ChangeReason",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Finance",
                table: "TaxCodeRateVersion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Finance",
                table: "TaxCodeMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActivated",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "UX_TaxCodeRateVersion_OpenPerCode",
                schema: "Finance",
                table: "TaxCodeRateVersion",
                column: "TaxCodeId",
                unique: true,
                filter: "[EffectiveTo] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_TaxCodeMaster_Company_Code",
                schema: "Finance",
                table: "TaxCodeMaster",
                columns: new[] { "CompanyId", "TaxCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage",
                columns: new[] { "CompanyId", "GlAccountId" },
                unique: true,
                filter: "[EffectiveTo] IS NULL AND [IsActivated] = 1 AND [IsDeleted] = 0");
        }
    }
}
