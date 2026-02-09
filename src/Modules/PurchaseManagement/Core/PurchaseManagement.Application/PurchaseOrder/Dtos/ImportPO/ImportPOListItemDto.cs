namespace PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO
{
    public sealed class ImportPOListItem
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = default!;
        public DateTimeOffset PODate { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
        public decimal PurchaseValue { get; set; }
        public int StatusId { get; set; }

        public int? ImportHeaderId { get; set; }
        public string? BillOfLadingNumber { get; set; }
        public string? AirWaybillNumber { get; set; }
        public string? ContainerNumber { get; set; }
    }

    public sealed class ImportPODetailReadDto
    {
        public int Id { get; set; }
        public int PurchaseHeaderId { get; set; }
        public int IndentId { get; set; }
        public string? IndentNumber { get; set; }
        public int ItemId { get; set; }
        public int ItemSno { get; set; }  
        public decimal Quantity { get; set; }
        public string? ItemName { get; set; }
        public int UomId { get; set; }
        public string? UomName { get; set; }
        public decimal UnitPrice { get; set; }
        public int DutyMasterId { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal CIFValue { get; set; }
        public decimal BasicCustomDuty { get; set; }
        public decimal SocialWelfareSurCharges { get; set; }
        public decimal IGST { get; set; }
        public decimal IGSTPercentage { get; set; }
        public decimal? AgriInfraDevCess { get; set; }
        public decimal? AntiDumpingDuty { get; set; }
        public decimal? SafeguardDuty { get; set; }
        public decimal? HealthEducationCess { get; set; }
        public decimal? OtherCharges { get; set; }
        public decimal TotalValue { get; set; }
        public bool GRBasedIV { get; set; }
    }

    public sealed class ImportPOHeaderReadDto
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int TTExchangeRateId { get; set; }
        public decimal TTExchangeRate { get; set; }
        public int? IncotermId { get; set; }
        public int? ShippingPortId { get; set; }
        public int? DestinationPortId { get; set; }
        public int? ModeOfTransportId { get; set; }
        public int ShippingModeId { get; set; }
        public int? CustomsOfficeId { get; set; }
        public int? OriginCountryId { get; set; }
        public int? InsuranceProviderId { get; set; }
        public int? FreightForwarderId { get; set; }
        public int? DocumentTypeId { get; set; }
        public int? FreeDaysAllowed { get; set; }
        public string? DemurrageTerms { get; set; }
        public string? BillOfLadingNumber { get; set; }
        public string? VesselName { get; set; }
        public string? ContainerNumber { get; set; }
        public string? AirlineName { get; set; }
        public string? AirWaybillNumber { get; set; }
        public DateTimeOffset? AirWaybillDate { get; set; }
        public string? FlightNumber { get; set; }
        public DateTimeOffset ExpectedTimeOfDeparture { get; set; }
        public DateTimeOffset ExpectedTimeOfArrival { get; set; }
        public int? CustomsHouseAgentId { get; set; }
        public string? BillOfEntryNumber { get; set; } = default!;
        public int? LCPaymentModeId { get; set; }
        public int? LCPaymentStatusId { get; set; }
        public string? LCNumber { get; set; }
        public string? LCCurrencyId { get; set; }
        public DateTimeOffset? LCDate { get; set; }
        public DateTimeOffset? LCExpiryDate { get; set; }
        public decimal? LCAmount { get; set; }
        public int? LCIssueBankId { get; set; }
        public int? LCBeneficiaryBankId { get; set; }
        public int? LCTypeId { get; set; }
        public string? LCRemarks { get; set; }
        public string? TTReferenceNumber { get; set; }
        public DateTimeOffset? TTTransferDate { get; set; }
        public int? TTBankId { get; set; }
        public int? TTCurrencyId { get; set; }
        public int? TTPaymentModeId { get; set; }
        public int? TTPaymentStatusId { get; set; }
        public string? TTRemarks { get; set; }
        public string? LCSwiftCode { get; set; }
        public string? TTSwiftCode { get; set; }
        public bool IsPartialReceiptAllowed { get; set; }
        public List<ImportPODetailReadDto> Details { get; set; } = new();
    }
    public sealed class PurchaseOrderHeaderSummaryDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string PONumber { get; set; } = default!;
        public DateTimeOffset PODate { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }
        public decimal ItemTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal PandFTotal { get; set; }
        public decimal MiscCharges { get; set; }
        public decimal GSTTotal { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal? IGSTTotal { get; set; }
        public decimal? FreightTotal { get; set; }
        public decimal? InsuranceTotal { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? PurchaseValue { get; set; }
        public int StatusId { get; set; }
        public int Edit { get; set; }
        public string? EditReason { get; set; }
        public int POCategoryId { get; set; }
        public int POMethodId { get; set; }
        public string? AmendmentReason { get; set; }
        public int RevisionNo { get; set; }
        public int CapitalTypeId { get; set; }
        public int CostCenterId { get; set; }
        public int ProjectId { get; set; }
        public int? WBSId { get; set; }
        public int PurchaseTypeId { get; set; }
        public int BudgetGroupId { get; set; }
        public int BudgetRequestById { get; set; } 
        public int? BudgetDepartmentId { get; set; } 

    }

    public sealed class PurchasePaymentTermReadDto
    {
        public int Id { get; set; }
        public int PaymentTermId { get; set; }
        public decimal? AdvancePercent { get; set; }
        public int? CreditDays { get; set; }
        public int? PaymentModelId { get; set; }
        public int? InsuranceId { get; set; }
        public int? InsurancePercent { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? BalancePercent { get; set; }
        public decimal? BalanceAmount { get; set; }
    }

    public sealed class ImportPOFullVm
    {
        public PurchaseOrderHeaderSummaryDto PO { get; set; } = default!;
        public List<PurchasePaymentTermReadDto> PaymentTerms { get; set; } = new();
        public List<ImportPOHeaderReadDto> ImportHeaders { get; set; } = new();
        public List<DocumentDtoList> ImportDocumentList { get; set; } = new();
    }

    public sealed class ImportPOAutocompleteDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = default!;
    }
    public class DocumentDtoList
    {
        public int DocumentId { get; set; }
        public string? FileName { get; set; }
        public DateTimeOffset UploadedDate { get; set; }
        public string? UploadedPath { get; set; }
        public string? BasePath { get; set; }
        public string? ImageFolder { get; set; }
        public string? DocumentName { get; set; }  
}
}
