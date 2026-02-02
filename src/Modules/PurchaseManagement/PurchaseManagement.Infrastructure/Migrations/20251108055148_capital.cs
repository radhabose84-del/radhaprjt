using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class capital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPODetail_DutyMaster_DutyMasterId",
                schema: "Purchase",
                table: "ImportPODetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPODetail_ImportPOHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_CustomsOfficeId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_IncotermId",
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
                name: "FK_ImportPOHeader_MiscMaster_ModeOfTransportId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_PortMaster_DestinationPortId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_PortMaster_ShippingPortId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportPOHeader",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportPODetail",
                schema: "Purchase",
                table: "ImportPODetail");

            migrationBuilder.DropColumn(
                name: "POImage",
                schema: "Purchase",
                table: "PurchaseLocalHeader");

            migrationBuilder.RenameTable(
                name: "ImportPOHeader",
                schema: "Purchase",
                newName: "PurchaseOrderImportHeader",
                newSchema: "Purchase");

            migrationBuilder.RenameTable(
                name: "ImportPODetail",
                schema: "Purchase",
                newName: "PurchaseOrderImportDetail",
                newSchema: "Purchase");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_TTPaymentStatusId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_TTPaymentStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_TTPaymentModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_TTPaymentModeId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_TTExchangeRateId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_TTExchangeRateId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_ShippingPortId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_ShippingPortId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_PurchaseOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_ModeOfTransportId_ExpectedTimeOfDeparture",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_ModeOfTransportId_ExpectedTimeOfDeparture");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_LCTypeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_LCTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_LCPaymentStatusId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_LCPaymentStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_LCPaymentModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_LCPaymentModeId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_IncotermId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_IncotermId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_DestinationPortId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_DestinationPortId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_CustomsOfficeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_CustomsOfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_BillOfLadingNumber",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_BillOfLadingNumber");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPOHeader_AirWaybillNumber",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                newName: "IX_PurchaseOrderImportHeader_AirWaybillNumber");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPODetail_PurchaseHeaderId",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail",
                newName: "IX_PurchaseOrderImportDetail_PurchaseHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_ImportPODetail_DutyMasterId",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail",
                newName: "IX_PurchaseOrderImportDetail_DutyMasterId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrderImportHeader",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrderImportDetail",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportDetail_DutyMaster_DutyMasterId",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail",
                column: "DutyMasterId",
                principalSchema: "Purchase",
                principalTable: "DutyMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportDetail_PurchaseOrderImportHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail",
                column: "PurchaseHeaderId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderImportHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "TTExchangeRateId",
                principalSchema: "Purchase",
                principalTable: "ExchangeRate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_CustomsOfficeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "CustomsOfficeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_IncotermId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "IncotermId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_LCPaymentModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "LCPaymentModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_LCPaymentStatusId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "LCPaymentStatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_LCTypeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "LCTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_ModeOfTransportId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "ModeOfTransportId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_TTPaymentModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "TTPaymentModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_TTPaymentStatusId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "TTPaymentStatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_PortMaster_DestinationPortId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "DestinationPortId",
                principalSchema: "Purchase",
                principalTable: "PortMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_PortMaster_ShippingPortId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "ShippingPortId",
                principalSchema: "Purchase",
                principalTable: "PortMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "PurchaseOrderId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportDetail_DutyMaster_DutyMasterId",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportDetail_PurchaseOrderImportHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_CustomsOfficeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_IncotermId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_LCPaymentModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_LCPaymentStatusId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_LCTypeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_ModeOfTransportId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_TTPaymentModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_TTPaymentStatusId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_PortMaster_DestinationPortId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_PortMaster_ShippingPortId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrderImportHeader",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrderImportDetail",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail");

            migrationBuilder.RenameTable(
                name: "PurchaseOrderImportHeader",
                schema: "Purchase",
                newName: "ImportPOHeader",
                newSchema: "Purchase");

            migrationBuilder.RenameTable(
                name: "PurchaseOrderImportDetail",
                schema: "Purchase",
                newName: "ImportPODetail",
                newSchema: "Purchase");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_TTPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_TTPaymentStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_TTPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_TTPaymentModeId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_TTExchangeRateId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_ShippingPortId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_ShippingPortId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_PurchaseOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_ModeOfTransportId_ExpectedTimeOfDeparture",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_ModeOfTransportId_ExpectedTimeOfDeparture");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_LCTypeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_LCTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_LCPaymentStatusId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_LCPaymentStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_LCPaymentModeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_LCPaymentModeId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_IncotermId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_IncotermId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_DestinationPortId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_DestinationPortId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_CustomsOfficeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_CustomsOfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_BillOfLadingNumber",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_BillOfLadingNumber");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportHeader_AirWaybillNumber",
                schema: "Purchase",
                table: "ImportPOHeader",
                newName: "IX_ImportPOHeader_AirWaybillNumber");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportDetail_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail",
                newName: "IX_ImportPODetail_PurchaseHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderImportDetail_DutyMasterId",
                schema: "Purchase",
                table: "ImportPODetail",
                newName: "IX_ImportPODetail_DutyMasterId");

            migrationBuilder.AddColumn<string>(
                name: "POImage",
                schema: "Purchase",
                table: "PurchaseLocalHeader",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportPOHeader",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportPODetail",
                schema: "Purchase",
                table: "ImportPODetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPODetail_DutyMaster_DutyMasterId",
                schema: "Purchase",
                table: "ImportPODetail",
                column: "DutyMasterId",
                principalSchema: "Purchase",
                principalTable: "DutyMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPODetail_ImportPOHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail",
                column: "PurchaseHeaderId",
                principalSchema: "Purchase",
                principalTable: "ImportPOHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTExchangeRateId",
                principalSchema: "Purchase",
                principalTable: "ExchangeRate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_CustomsOfficeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "CustomsOfficeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_MiscMaster_IncotermId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "IncotermId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

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
                name: "FK_ImportPOHeader_MiscMaster_ModeOfTransportId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "ModeOfTransportId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_PortMaster_DestinationPortId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "DestinationPortId",
                principalSchema: "Purchase",
                principalTable: "PortMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_PortMaster_ShippingPortId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "ShippingPortId",
                principalSchema: "Purchase",
                principalTable: "PortMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "PurchaseOrderId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
