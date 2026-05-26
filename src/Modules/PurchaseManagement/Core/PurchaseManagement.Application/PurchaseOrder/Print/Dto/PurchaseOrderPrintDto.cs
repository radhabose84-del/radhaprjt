namespace PurchaseManagement.Application.PurchaseOrder.Print.Dto;

// ──────────────────────────── Root DTO ────────────────────────────

public class PurchaseOrderPrintDto
{
    public string? POType { get; set; }
    public POPrintCompanyDto? Company { get; set; }
    public POPrintRegisteredOfficeDto? RegisteredOffice { get; set; }
    public POPrintHeaderDto? PurchaseOrder { get; set; }
    public POPrintVendorDto? Vendor { get; set; }
    public POPrintDeliveryDto? Delivery { get; set; }
    public List<POPrintItemDto>? Items { get; set; }
    public List<POPrintServiceLineDto>? ServiceLines { get; set; }
    public POPrintTotalsDto? Totals { get; set; }
    public List<POPrintPaymentTermDto>? PaymentTerms { get; set; }
    public POPrintBankDto? Bank { get; set; }
    public POPrintImportDto? ImportDetails { get; set; }
    public POPrintContractDto? ContractDetails { get; set; }
    public POPrintServiceHeaderDto? ServiceDetails { get; set; }
}

// ──────────────────────────── Section DTOs ────────────────────────────

public class POPrintCompanyDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Gstin { get; set; }
    public string? Pan { get; set; }
    public string? Cin { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }
    public string? Phone { get; set; }
}

public class POPrintRegisteredOfficeDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
}

public class POPrintHeaderDto
{
    public string? PONumber { get; set; }
    public string? PODate { get; set; }
    public string? POCategory { get; set; }
    public string? POMethod { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public string? Status { get; set; }
    public int RevisionNo { get; set; }
    public string? AmendmentReason { get; set; }
    public string? Incoterms { get; set; }
    public string? ModeOfDispatch { get; set; }
    public decimal? FreightCharges { get; set; }
    public string? TermDescription { get; set; }
}

public class POPrintVendorDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? StateCode { get; set; }
    public string? Gstin { get; set; }
    public string? Pan { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class POPrintDeliveryDto
{
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
    public bool IsPartialReceiptAllowed { get; set; }
}

public class POPrintItemDto
{
    public int SNo { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public string? HSNCode { get; set; }
    public string? UOMName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemValue { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? GSTPercentage { get; set; }
    public decimal? CGSTPercentage { get; set; }
    public decimal? SGSTPercentage { get; set; }
    public decimal? IGSTPercentage { get; set; }
    public decimal? CGST { get; set; }
    public decimal? SGST { get; set; }
    public decimal? IGST { get; set; }
    public string? ScheduleDate { get; set; }
    public string? DepartmentName { get; set; }
    // Import-specific (nullable for non-import)
    public decimal? FreightAmount { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public decimal? CIFValue { get; set; }
    public decimal? BasicCustomDuty { get; set; }
    public decimal? TotalValue { get; set; }
}

public class POPrintServiceLineDto
{
    public int LineNo { get; set; }
    public string? ServiceCode { get; set; }
    public string? ServiceDescription { get; set; }
    public string? UOMName { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal PlannedRate { get; set; }
    public decimal PlannedValue { get; set; }
    public decimal? Discount { get; set; }
    public decimal? GstPercent { get; set; }
    public string? Remarks { get; set; }
}

public class POPrintTotalsDto
{
    public decimal ItemTotal { get; set; }
    public decimal? DiscountTotal { get; set; }
    public decimal? PandFTotal { get; set; }
    public decimal? MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal? FreightTotal { get; set; }
    public decimal? InsuranceTotal { get; set; }
    public decimal? TDSTotal { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal PurchaseValue { get; set; }
    public string? PurchaseValueInWords { get; set; }
}

public class POPrintPaymentTermDto
{
    public string? PaymentTermName { get; set; }
    public decimal? AdvancePercent { get; set; }
    public int? CreditDays { get; set; }
    public string? PaymentModeName { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
}

public class POPrintBankDto
{
    public string? Name { get; set; }
    public string? Branch { get; set; }
    public string? AccountNo { get; set; }
    public string? Ifsc { get; set; }
}

// ──────────────────────── Type-Specific DTOs ─────────────────────────

public class POPrintImportDto
{
    public decimal? TTExchangeRate { get; set; }
    public string? ShippingPortName { get; set; }
    public string? DestinationPortName { get; set; }
    public string? ModeOfTransportName { get; set; }
    public string? ShippingModeName { get; set; }
    public string? OriginCountryName { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public string? VesselName { get; set; }
    public string? ContainerNumber { get; set; }
    public string? ExpectedDeparture { get; set; }
    public string? ExpectedArrival { get; set; }
    public string? LCNumber { get; set; }
    public string? LCDate { get; set; }
    public decimal? LCAmount { get; set; }
    public string? TTReferenceNumber { get; set; }
    public string? TTTransferDate { get; set; }
}

public class POPrintContractDto
{
    public string? ContractPONumber { get; set; }
    public int ContractPOHeaderId { get; set; }
}

public class POPrintServiceHeaderDto
{
    public string? ServiceCategoryName { get; set; }
    public string? ContractTypeName { get; set; }
    public string? FrequencyName { get; set; }
    public string? ValidityFrom { get; set; }
    public string? ValidityTo { get; set; }
    public int? TotalOccurrences { get; set; }
    public decimal? OverallLimit { get; set; }
}

// ──────────────────── Raw Dapper Mapping DTOs ────────────────────────

public class POHeaderRawDto
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string? PONumber { get; set; }
    public DateTimeOffset PODate { get; set; }
    public int POCategoryId { get; set; }
    public string? POCategoryCode { get; set; }
    public string? POCategoryDescription { get; set; }
    public int? POMethodId { get; set; }
    public string? POMethodCode { get; set; }
    public string? POMethodDescription { get; set; }
    public int CurrencyId { get; set; }
    public int VendorId { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal? DiscountTotal { get; set; }
    public decimal? PandFTotal { get; set; }
    public decimal? MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal? FreightTotal { get; set; }
    public decimal? InsuranceTotal { get; set; }
    public decimal? TDSTotal { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal PurchaseValue { get; set; }
    public int StatusId { get; set; }
    public string? StatusDescription { get; set; }
    public int RevisionNo { get; set; }
    public string? AmendmentReason { get; set; }
}

public class LocalHeaderRawDto
{
    public int Id { get; set; }
    public bool IsPartialReceiptAllowed { get; set; }
    public string? IncotermsName { get; set; }
    public string? ModeOfDispatchName { get; set; }
    public decimal? FreightCharges { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
}

public class LocalDetailRawDto
{
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemValue { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? GSTPercentage { get; set; }
    public decimal? CGSTPercentage { get; set; }
    public decimal? SGSTPercentage { get; set; }
    public decimal? IGSTPercentage { get; set; }
    public decimal? CGST { get; set; }
    public decimal? SGST { get; set; }
    public decimal? IGST { get; set; }
    public DateTimeOffset? ScheduleDate { get; set; }
    public int? DepartmentId { get; set; }
}

public class ContractHeaderRawDto
{
    public int Id { get; set; }
    public int ContractPOHeaderId { get; set; }
    public string? ContractPONumber { get; set; }
    public bool IsPartialReceiptAllowed { get; set; }
    public string? IncotermsName { get; set; }
    public string? ModeOfDispatchName { get; set; }
    public decimal? FreightCharges { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
}

public class ContractDetailRawDto
{
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemValue { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? GSTPercentage { get; set; }
    public decimal? CGSTPercentage { get; set; }
    public decimal? SGSTPercentage { get; set; }
    public decimal? IGSTPercentage { get; set; }
    public decimal? CGST { get; set; }
    public decimal? SGST { get; set; }
    public decimal? IGST { get; set; }
    public DateTimeOffset? ScheduleDate { get; set; }
    public int? DepartmentId { get; set; }
}

public class ImportHeaderRawDto
{
    public int Id { get; set; }
    public decimal? TTExchangeRate { get; set; }
    public string? IncotermsName { get; set; }
    public string? ShippingPortName { get; set; }
    public string? DestinationPortName { get; set; }
    public string? ModeOfTransportName { get; set; }
    public string? ShippingModeName { get; set; }
    public int? OriginCountryId { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public string? VesselName { get; set; }
    public string? ContainerNumber { get; set; }
    public DateTimeOffset ExpectedTimeOfDeparture { get; set; }
    public DateTimeOffset ExpectedTimeOfArrival { get; set; }
    public string? LCNumber { get; set; }
    public DateTimeOffset? LCDate { get; set; }
    public decimal? LCAmount { get; set; }
    public string? TTReferenceNumber { get; set; }
    public DateTimeOffset? TTTransferDate { get; set; }
    public bool IsPartialReceiptAllowed { get; set; }
    public string? ModeOfDispatchName { get; set; }
    public decimal? FreightCharges { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
}

public class ImportDetailRawDto
{
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UomId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? FreightAmount { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public decimal? CIFValue { get; set; }
    public decimal? BasicCustomDuty { get; set; }
    public decimal? IGSTPercentage { get; set; }
    public decimal IGST { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal TotalValue { get; set; }
}

public class ServiceHeaderRawDto
{
    public int Id { get; set; }
    public string? ServiceCategoryName { get; set; }
    public string? ContractTypeName { get; set; }
    public string? FrequencyName { get; set; }
    public DateTimeOffset? ValidityFrom { get; set; }
    public DateTimeOffset? ValidityTo { get; set; }
    public int? TotalOccurrences { get; set; }
    public decimal? OverallLimit { get; set; }
    public string? ModeOfDispatchName { get; set; }
    public decimal? FreightCharges { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }
}

public class ServiceLineRawDto
{
    public int LineNo { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceCode { get; set; }
    public string? ServiceDescription { get; set; }
    public int? UOMId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal PlannedRate { get; set; }
    public decimal PlannedValue { get; set; }
    public decimal? Discount { get; set; }
    public decimal? GstPercent { get; set; }
    public string? Remarks { get; set; }
}

public class PaymentTermRawDto
{
    public string? PaymentTermName { get; set; }
    public decimal? AdvancePercent { get; set; }
    public int? CreditDays { get; set; }
    public string? PaymentModeName { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? BalanceAmount { get; set; }
}
