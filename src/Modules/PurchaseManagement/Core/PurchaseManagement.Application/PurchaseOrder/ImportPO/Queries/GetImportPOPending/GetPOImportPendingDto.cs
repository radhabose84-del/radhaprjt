public sealed class GetPOImportPendingGroupDto
{
    public int PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = default!;
    public DateTimeOffset PODate { get; set; }
    public int VendorId { get; set; }
     public string? VendorName { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorMobile { get; set; }
    public decimal PurchaseValue { get; set; }
    public int StatusId { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal PandFTotal { get; set; }
    public decimal MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal FreightTotal { get; set; }
    public decimal InsuranceTotal { get; set; }
    public decimal TDSTotal { get; set; }
    public decimal AdvanceAmount { get; set; }
    public DateTimeOffset createdDate { get; set; }
    public string? createdByName { get; set; }
    public string? StatusCode { get; set; }
    public string? POCategoryCode { get; set; }
    public string? POMethodCode { get; set; }

    // Useful Import fields (from any header)
    public int? ImportHeaderId { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public string? AirWaybillNumber { get; set; }
    public string? ContainerNumber { get; set; }    
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; }  

    public List<GetPOImportPendingDto>? Lines { get; set; }
}

public sealed class GetPOImportPendingDto
{
    public int Id { get; set; }
    public int PurchaseHeaderId { get; set; }  // ImportPOHeader.Id
    public int IndentId { get; set; }
    public int ItemId { get; set; }
    public String? ItemName { get; set; }
    public int UomId { get; set; }
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
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public decimal ItemValue { get; set; }
}
