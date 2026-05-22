namespace PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

public sealed class PurchaseLocalDetailDto
{
    public int? Id { get; set; }
    public int PurchaseLocalId { get; set; }
    public int? IndentId { get; set; }
    public int ItemId { get; set; }
    public int ItemSno { get; set; }
    public int UOMId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? LastPOPrice { get; set; }
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
    public decimal ItemValue { get; set; }
    public string? UOMName { get; set; }
    public string? ItemName { get; set; }
    public string? DepartmentName { get; set; } 
    public string? IndentNumber { get; set; }
    
}

public sealed class PurchaseLocalHeaderDto
{
    public int? Id { get; set; }
    public int PurchaseOrderId { get; set; } 
    public bool IsPartialReceiptAllowed { get; set; }
    public int? IncotermsId { get; set; }
    public int? ModeOfDispatchId { get; set; }
    public decimal? FreightCharges { get; set; }
    public int? TermsId { get; set; }
    public string? TermDescription { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? BillingAddress { get; set; }    
    public string? ImageUrl { get; set; }    
    
    public List<PurchaseLocalDetailDto> Details { get; set; } = new();
}

public sealed class PurchasePaymentTermDto
{
    public int? Id { get; set; }
    public int PurchaseOrderId { get; set; } 
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

public class PurchaseOrderCreateDto
{
    public int Id { get; set; }
    public string PONumber { get; set; } = default!;
    public DateTimeOffset PODate { get; set; }
    public int POCategoryId { get; set; }
    public int? POMethodId { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal PandFTotal { get; set; }
    public decimal MiscCharges { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal FreightTotal { get; set; }
    public decimal InsuranceTotal { get; set; }
    public decimal TDSTotal { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal PurchaseValue { get; set; }
    public int RevisionNo { get; set; }
    public string? AmendmentReason { get; set; }
    public int Edit { get; set; }
    public string? EditReason { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public int? WBSId { get; set; }
    public int? CapitalTypeId { get; set; }
    public int? PurchaseTypeId { get; set; }
    public int? BudgetGroupId { get; set; }
    public int? BudgetRequestById { get; set; } 
    public int? BudgetMonthId { get; set; } 
    public int? FinancialYearId { get; set; } 
    public int? BudgetDepartmentId { get; set; }
    public int? ItemCategoryId { get; set; }

    public List<PurchaseLocalHeaderDto> Headers { get; set; } = new();
    public List<PurchasePaymentTermDto> PaymentTerms { get; set; } = new();
    public List<PurchaseDocumentDto> Documents { get; set; } = new();
}

public class    PurchaseOrderUpdateDto : PurchaseOrderCreateDto
{
    public new int Id { get; set; }
}

public sealed class PurchaseOrderListItemDto
{
    public int Id { get; set; }
    public string PONumber { get; set; } = default!;
    public DateTimeOffset PODate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public decimal PurchaseValue { get; set; }
    public int StatusId { get; set; }
    public string? StatusCode { get; set; }
    public string? POCategoryCode { get; set; }
    public string? POMethodCode { get; set; }
    public int POMethodId { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal PandFTotal { get; set; }
    public decimal MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal GSTPercentage { get; set; }
    public decimal CGSTPercentage { get; set; }
    public decimal SGSTPercentage { get; set; }
    public decimal IGSTPercentage { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal FreightTotal { get; set; }
    public decimal InsuranceTotal { get; set; }
    public decimal TDSTotal { get; set; }
    public decimal AdvanceAmount { get; set; }
    public int? Edit { get; set; }
    public string? EditReason { get; set; }
    public string? UOMName { get; set; }
    public int RevisionNo { get; set; }
    public string? AmendmentReason { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public int? WBSId { get; set; }
    public int? CapitalTypeId { get; set; }
    public int? PurchaseTypeId { get; set; }
    public int? BudgetGroupId { get; set; }
    public string? BudgetGroupName { get; set; }
    public int? BudgetRequestById { get; set; }
    public int? BudgetDepartmentId { get; set; }
    public int? ItemCategoryId { get; set; }
    public string? ItemCategoryName { get; set; }

    // GRN flag (like DAFlag in SalesOrder)
    public string? GRNFlag { get; set; }

    // Cancel / Foreclose status flags
    public bool IsCancelled { get; set; }
    public bool IsForeclosed { get; set; }

    // Cancel / Foreclose eligibility flags
    public bool CanCancel { get; set; }
    public bool CanForeclose { get; set; }

    // Cancel details
    public DateTimeOffset? CancelledDate { get; set; }
    public string? CancelledByName { get; set; }
    public string? CancelledIP { get; set; }

    // Foreclose details
    public DateTimeOffset? ForeClosedDate { get; set; }
    public string? ForeClosedByName { get; set; }
    public string? ForeClosedIP { get; set; }
}

public sealed class PurchaseOrderDetailDto : PurchaseOrderUpdateDto
{
    public int UnitId { get; set; }
    public int StatusId { get; set; }
    public new List<PurchaseLocalHeaderDto> Headers { get; set; } = new();
    public new List<PurchasePaymentTermDto> PaymentTerms { get; set; } = new();
    #pragma warning disable CS0109
    public new List<LocalDocumentDto> DocumentsList { get; set; } = new();
    #pragma warning restore CS0109
}

public sealed class AutocompleteDto
{
    public int Id { get; set; }
    public string Label { get; set; } = default!;
}
  public sealed class PurchaseDocumentDto
    {
        public int DocumentId { get; set; }          
        public string FileName { get; set; } = "";   
        public DateTimeOffset UploadedDate { get; set; }              
    }
/* public sealed class UpdatePODocumentDto
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = "";
    public DateTimeOffset UploadedDate { get; set; }
} */
    public class LocalDocumentDto
    {
        public int DocumentId { get; set; }
        public string? FileName { get; set; }
        public DateTimeOffset UploadedDate { get; set; }
        public string? UploadedPath { get; set; }          
        public string? DocumentName { get; set; }  
}