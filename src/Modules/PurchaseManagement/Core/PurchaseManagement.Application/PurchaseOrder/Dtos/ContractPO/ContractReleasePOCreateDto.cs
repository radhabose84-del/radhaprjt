namespace PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

public class ContractReleasePOCreateDto
{
    // Contract reference
    public int ContractPOHeaderId { get; set; }

    // PO header fields
    public int UnitId { get; set; }
    public DateTimeOffset PODate { get; set; }
    public int POCategoryId { get; set; }
    public int? POMethodId { get; set; }
    public int StatusId { get; set; }

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

    // Release details
    public List<ContractReleasePODetailItem> Details { get; set; } = new();
}

public class ContractReleasePOUpdateDto : ContractReleasePOCreateDto
{
    public int Id { get; set; }
    public int RevisionNo { get; set; }
    public string? AmendmentReason { get; set; }
}

public sealed class ContractReleasePODetailVm : ContractReleasePOUpdateDto
{
    public string? PONumber { get; set; }
    public string? VendorName { get; set; }
    public string? CurrencyName { get; set; }
    public string? ContractPONumber { get; set; }
}

public sealed class ContractReleasePODetailItem
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
