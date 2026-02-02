using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BnkDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DutyMaster",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DutyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TariffNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DutyCategoryId = table.Column<int>(type: "int", nullable: false),
                    BasicCustomsDutyPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SocialWelfareSurchargePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IGSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AgriInfraDevCessPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AntiDumpingDutyPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    SafeguardDutyPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    HealthEducationCessPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    TotalDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CountryOfOriginApplicability = table.Column<int>(type: "int", nullable: false),
                    NotificationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_DutyMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DutyMaster_MiscMaster_CountryOfOriginApplicability",
                        column: x => x.CountryOfOriginApplicability,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DutyMaster_MiscMaster_DutyCategoryId",
                        column: x => x.DutyCategoryId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImportPOHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRateId = table.Column<int>(type: "int", nullable: false),
                    IncotermId = table.Column<int>(type: "int", nullable: false),
                    ShippingPortId = table.Column<int>(type: "int", nullable: false),
                    DestinationPortId = table.Column<int>(type: "int", nullable: false),
                    ModeOfTransportId = table.Column<int>(type: "int", nullable: false),
                    CustomsOfficeId = table.Column<int>(type: "int", nullable: false),
                    OriginCountryId = table.Column<int>(type: "int", nullable: false),
                    InsuranceProviderId = table.Column<int>(type: "int", nullable: true),
                    FreightForwarderId = table.Column<int>(type: "int", nullable: true),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: true),
                    FreeDaysAllowed = table.Column<int>(type: "int", nullable: true),
                    DemurrageTerms = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    BillOfLadingNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    VesselName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ContainerNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AirlineName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    AirWaybillNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AirWaybillDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FlightNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ExpectedTimeOfDeparture = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpectedTimeOfArrival = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CustomsHouseAgentId = table.Column<int>(type: "int", nullable: false),
                    BillOfEntryNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LCNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    LCDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LCExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LCAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IssueBankId = table.Column<int>(type: "int", nullable: true),
                    BeneficiaryBankId = table.Column<int>(type: "int", nullable: true),
                    LCTypeId = table.Column<int>(type: "int", nullable: true),
                    TTReferenceNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TransferDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BankId = table.Column<int>(type: "int", nullable: true),
                    PaymentStatusId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
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
                    table.PrimaryKey("PK_ImportPOHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportPOHeader_ExchangeRate_ExchangeRateId",
                        column: x => x.ExchangeRateId,
                        principalSchema: "Purchase",
                        principalTable: "ExchangeRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportPOHeader_MiscMaster_CustomsOfficeId",
                        column: x => x.CustomsOfficeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportPOHeader_MiscMaster_IncotermId",
                        column: x => x.IncotermId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportPOHeader_MiscMaster_ModeOfTransportId",
                        column: x => x.ModeOfTransportId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportPOHeader_PortMaster_DestinationPortId",
                        column: x => x.DestinationPortId,
                        principalSchema: "Purchase",
                        principalTable: "PortMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportPOHeader_PortMaster_ShippingPortId",
                        column: x => x.ShippingPortId,
                        principalSchema: "Purchase",
                        principalTable: "PortMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportPOHeader_PurchaseOrderHeader_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseDocuments",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseDocuments_MiscMaster_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportPODetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseHeaderId = table.Column<int>(type: "int", nullable: false),
                    IndentId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DutyMasterId = table.Column<int>(type: "int", nullable: false),
                    FreightAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InsuranceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CIFValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BasicCustomDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SocialWelfareSurCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AgriInfraDevCess = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AntiDumpingDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SafeguardDuty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HealthEducationCess = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GRBasedIV = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportPODetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportPODetail_DutyMaster_DutyMasterId",
                        column: x => x.DutyMasterId,
                        principalSchema: "Purchase",
                        principalTable: "DutyMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ImportPODetail_ImportPOHeader_PurchaseHeaderId",
                        column: x => x.PurchaseHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "ImportPOHeader",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DutyMaster_CountryOfOriginApplicability",
                schema: "Purchase",
                table: "DutyMaster",
                column: "CountryOfOriginApplicability");

            migrationBuilder.CreateIndex(
                name: "IX_DutyMaster_DutyCategoryId",
                schema: "Purchase",
                table: "DutyMaster",
                column: "DutyCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPODetail_DutyMasterId",
                schema: "Purchase",
                table: "ImportPODetail",
                column: "DutyMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPODetail_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail",
                column: "PurchaseHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_AirWaybillNumber",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "AirWaybillNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_BillOfLadingNumber",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "BillOfLadingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_CustomsOfficeId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "CustomsOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_DestinationPortId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "DestinationPortId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_ExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "ExchangeRateId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_IncotermId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "IncotermId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_ModeOfTransportId_ExpectedTimeOfDeparture",
                schema: "Purchase",
                table: "ImportPOHeader",
                columns: new[] { "ModeOfTransportId", "ExpectedTimeOfDeparture" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPOHeader_ShippingPortId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "ShippingPortId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDocuments_DocumentId",
                schema: "Purchase",
                table: "PurchaseDocuments",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportPODetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseDocuments",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "DutyMaster",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "ImportPOHeader",
                schema: "Purchase");
        }
    }
}
