using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImportPO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_ExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.RenameColumn(
                name: "TransferDate",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TTTransferDate");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TTRemarks");

            migrationBuilder.RenameColumn(
                name: "PaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TTPaymentStatusId");

            migrationBuilder.RenameColumn(
                name: "IssueBankId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TTPaymentModeId");

            migrationBuilder.RenameColumn(
                name: "ExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TTExchangeRateId");

            migrationBuilder.RenameColumn(
                name: "DocumentTypeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TTCurrencyId");

            migrationBuilder.RenameColumn(
                name: "BeneficiaryBankId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TTBankId");

            migrationBuilder.RenameColumn(
                name: "BankId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "LCPaymentStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_ExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_TTExchangeRateId");

            migrationBuilder.AddColumn<int>(
                name: "LCBeneficiaryBankId",
                schema: "Purchase",
                table: "ImportPOHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LCCurrencyId",
                schema: "Purchase",
                table: "ImportPOHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LCIssueBankId",
                schema: "Purchase",
                table: "ImportPOHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LCPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LCRemarks",
                schema: "Purchase",
                table: "ImportPOHeader",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_LCPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "LCPaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_LCPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "LCPaymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_LCTypeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "LCTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_TTPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTPaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_TTPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTPaymentStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTExchangeRateId",
                principalSchema: "Purchase",
                principalTable: "ExchangeRate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_LCPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "LCPaymentModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_LCPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "LCPaymentStatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_LCTypeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "LCTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_TTPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTPaymentModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_TTPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTPaymentStatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_LCPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_LCPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_LCTypeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_TTPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_TTPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropIndex(
                name: "IX_ImportPOHeader_LCPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropIndex(
                name: "IX_ImportPOHeader_LCPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropIndex(
                name: "IX_ImportPOHeader_LCTypeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropIndex(
                name: "IX_ImportPOHeader_TTPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropIndex(
                name: "IX_ImportPOHeader_TTPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropColumn(
                name: "LCBeneficiaryBankId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropColumn(
                name: "LCCurrencyId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropColumn(
                name: "LCIssueBankId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropColumn(
                name: "LCPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropColumn(
                name: "LCRemarks",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.RenameColumn(
                name: "TTTransferDate",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "TransferDate");

            migrationBuilder.RenameColumn(
                name: "TTRemarks",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "Remarks");

            migrationBuilder.RenameColumn(
                name: "TTPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "PaymentStatusId");

            migrationBuilder.RenameColumn(
                name: "TTPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IssueBankId");

            migrationBuilder.RenameColumn(
                name: "TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "ExchangeRateId");

            migrationBuilder.RenameColumn(
                name: "TTCurrencyId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "DocumentTypeId");

            migrationBuilder.RenameColumn(
                name: "TTBankId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "BeneficiaryBankId");

            migrationBuilder.RenameColumn(
                name: "LCPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "BankId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_ExchangeRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_ExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "ExchangeRateId",
                principalSchema: "Purchase",
                principalTable: "ExchangeRate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
