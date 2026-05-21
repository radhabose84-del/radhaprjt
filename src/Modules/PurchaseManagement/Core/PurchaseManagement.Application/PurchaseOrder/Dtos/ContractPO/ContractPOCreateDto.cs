namespace PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

public class ContractPOCreateDto
{
    // Contract reference
    public int ContractPOHeaderId { get; set; }

    // PO header fields (UnitId fetched from IIPAddressService, StatusId defaults to Pending)
    public DateTimeOffset PODate { get; set; }
    public int POCategoryId { get; set; }
    public int? POMethodId { get; set; }

    // PO totals (calculated by frontend from details)
    public decimal ItemTotal { get; set; }
    public decimal? DiscountTotal { get; set; }
    public decimal? PandFTotal { get; set; }
    public decimal? MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal? FreightTotal { get; set; }
    public decimal PurchaseValue { get; set; }

    // PurchaseContractHeader fields
    public bool IsPartialReceiptAllowed { get; set; }
    public int? IncotermsId { get; set; }
    public int? ModeOfDispatchId { get; set; }
    public decimal? FreightCharges { get; set; }
    public int? TermsId { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }

    // Budget fields
    public int? BudgetGroupId { get; set; }
    public int? BudgetMonthId { get; set; }
    public int? BudgetRequestById { get; set; }
    public int? ProjectId { get; set; }
    public int? WBSId { get; set; }
    public int? FinancialYearId { get; set; }

    // Release details
    public List<ContractPODetailItem> Details { get; set; } = new();

    // Documents
    public List<ContractPODocumentDto>? Documents { get; set; }
}

public class ContractPOUpdateDto : ContractPOCreateDto
{
    public int Id { get; set; }
    public int RevisionNo { get; set; }
    public string? AmendmentReason { get; set; }
}

public sealed class ContractPODetailVm : ContractPOUpdateDto
{
    public int UnitId { get; set; }
    public int StatusId { get; set; }
    public string? PONumber { get; set; }
    public string? VendorName { get; set; }
    public string? CurrencyName { get; set; }
    public string? ContractPONumber { get; set; }
}

public sealed class ContractPODetailItem
{
    public int ContractPODetailId { get; set; }
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemValue { get; set; }
    public int? DiscountTypeId { get; set; }
    public decimal? DiscountValue { get; set; }
    public int? PandFType { get; set; }
    public decimal? PandFCharge { get; set; }
    public decimal? OtherCharge { get; set; }
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

public sealed class ContractPODocumentDto
{
    public int DocumentId { get; set; }
    public string? FileName { get; set; }
    public DateTimeOffset UploadedDate { get; set; }
}
