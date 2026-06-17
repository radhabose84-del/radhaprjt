using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaxAccountLinkage_OldTaxLinkageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_TaxCodeMaster_OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropIndex(
                name: "IX_TaxAccountLinkage_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.RenameColumn(
                name: "OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "OldTaxLinkageId");

            migrationBuilder.RenameIndex(
                name: "IX_TaxAccountLinkage_OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "IX_TaxAccountLinkage_OldTaxLinkageId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_TaxAccountLinkage_OldTaxLinkageId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldTaxLinkageId",
                principalSchema: "Finance",
                principalTable: "TaxAccountLinkage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_TaxAccountLinkage_OldTaxLinkageId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.RenameColumn(
                name: "OldTaxLinkageId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "OldTaxCodeId");

            migrationBuilder.RenameIndex(
                name: "IX_TaxAccountLinkage_OldTaxLinkageId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                newName: "IX_TaxAccountLinkage_OldTaxCodeId");

            migrationBuilder.AddColumn<int>(
                name: "OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxAccountLinkage_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldControlAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldControlAccountId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_TaxCodeMaster_OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldTaxCodeId",
                principalSchema: "Finance",
                principalTable: "TaxCodeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
