namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES
{
    public class ServiceEntrySheetDetailDto
    {
           public SesDto? Ses { get; set; }

        // 🔹 PO Header
        public PurchaseOrderHeaderDto? PurchaseOrderHeader { get; set; }

        // 🔹 Other related collections
        public List<PaymentTermDto> PaymentTerms { get; set; } = new();
        public List<ServiceHeaderDto> ServiceHeaders { get; set; } = new();
        public List<ServiceLineDto> ServiceLines { get; set; } = new();
        public List<ServiceScheduleDto> ServiceSchedules { get; set; } = new();
        public List<ActivityDto> Activities { get; set; } = new();
        public List<DocumentDto> Documents { get; set; } = new();

        // ========== CHILD DTOS ==========

        // SES
        public class SesDto
        {
            public int Id { get; set; }
            public DateTimeOffset SESDate { get; set; }
            public int SESStatusId { get; set; }
            public int PurchaseOrderId { get; set; }
            public DateTimeOffset PODate { get; set; }
            public int VendorId { get; set; }
            public int? ServiceCategoryId { get; set; }
            public int? ContractTypeId { get; set; }
            public DateTimeOffset? ValidityFrom { get; set; }
            public DateTimeOffset? ValidityTo { get; set; }
            public int UnitId { get; set; }
            public string? AttachmentFileName { get; set; }
            public int ServiceId { get; set; }
            public string ServiceDescription { get; set; } = string.Empty;
            public int ScheduleId { get; set; }
            public int OccurrenceNo { get; set; }
            public string? OccurrencePeriod { get; set; }
            public DateTimeOffset ScheduleStartDate { get; set; }
            public DateTimeOffset ScheduleEndDate { get; set; }
            public decimal ActualQuantity { get; set; }
            public decimal ActualRate { get; set; }
            public decimal ActualValue { get; set; }
            public int? DiscountTypeId { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal TaxPercentage { get; set; }
            public decimal TaxValue { get; set; }
            public decimal TotalValue { get; set; }
            public DateTimeOffset? WorkStartDate { get; set; }
            public DateTimeOffset? WorkEndDate { get; set; }
            public decimal? DurationHrs { get; set; }
            public string? LineRemarks { get; set; }
            public int StatusId { get; set; }
        }

        // Activity
        public class ActivityDto
        {
            public int Id { get; set; }
            public int EntrySheetId { get; set; }
            public int? ActivityTypeId { get; set; }
            public string? Description { get; set; }
            public string? PerformedByName { get; set; }
            public int? SESActivityStatusId { get; set; }
            public string? StatusRemarks { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
            public string? CreatedByName { get; set; }
            public string? CreatedIP { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTimeOffset? ModifiedDate { get; set; }
            public string? ModifiedByName { get; set; }
            public string? ModifiedIP { get; set; }
        }

         public class DocumentDto
        {   
            public int Id { get; set; }
            public int ServiceEntrySheetId { get; set; }
            public int DocumentId { get; set; }
            public string FileName { get; set; } = null!;
            public DateTimeOffset UploadedDate { get; set; }
            public string? UploadedPath { get; set; }
            public string? DocumentName { get; set; }            
        
        }

        // Purchase Order Header
        public class PurchaseOrderHeaderDto
        {
            public int Id { get; set; }
            public int UnitId { get; set; }
            public string PONumber { get; set; } = string.Empty;
            public DateTimeOffset PODate { get; set; }
            public int POCategoryId { get; set; }
            public int POMethodId { get; set; }
            public int CurrencyId { get; set; }
            public int VendorId { get; set; }
            public decimal ItemTotal { get; set; }
            public decimal DiscountTotal { get; set; }
            public decimal PandFTotal { get; set; }
            public decimal MiscCharges { get; set; }
            public decimal GSTTotal { get; set; }
            public decimal CGSTTotal { get; set; }
            public decimal SGSTTotal { get; set; }
            public decimal IGSTTotal { get; set; }
            public decimal FreightTotal { get; set; }
            public decimal InsuranceTotal { get; set; }
            public decimal TDSTotal { get; set; }
            public decimal AdvanceAmount { get; set; }
            public decimal PurchaseValue { get; set; }
            public int StatusId { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
            public string? CreatedByName { get; set; }
            public string? CreatedIP { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTimeOffset? ModifiedDate { get; set; }
            public string? ModifiedByName { get; set; }
            public string? ModifiedIP { get; set; }
            public string? AmendmentReason { get; set; }
            public int? OldPOId { get; set; }
            public int? RevisionNo { get; set; }
            public int? CapitalTypeId { get; set; }
            public int? CostCenterId { get; set; }
            public int? ProjectId { get; set; }
            public int? PurchaseTypeId { get; set; }
        }

        // Payment Term
        public class PaymentTermDto
        {
            public int Id { get; set; }
            public int PurchaseOrderId { get; set; }
            public int PaymentTermId { get; set; }
            public decimal AdvancePercent { get; set; }
            public int CreditDays { get; set; }
            public int PaymentModelId { get; set; }
            public int? InsuranceId { get; set; }
            public decimal InsurancePercent { get; set; }
            public decimal InsuranceAmount { get; set; }
            public decimal AdvanceAmount { get; set; }
            public decimal BalancePercent { get; set; }
            public decimal BalanceAmount { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
            public string? CreatedByName { get; set; }
            public string? CreatedIP { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTimeOffset? ModifiedDate { get; set; }
            public string? ModifiedByName { get; set; }
            public string? ModifiedIP { get; set; }
        }

        // Service Header
        public class ServiceHeaderDto
        {
            public int Id { get; set; }
            public int PurchaseOrderId { get; set; }
            public int ServiceCategoryId { get; set; }
            public int ContractTypeId { get; set; }
            public int? FrequencyId { get; set; }
            public DateTimeOffset? ValidityFrom { get; set; }
            public DateTimeOffset? ValidityTo { get; set; }
            public int? TotalOccurrences { get; set; }
            public decimal? OverallLimit { get; set; }
            public string? TermDescription { get; set; }
            public string? POImage { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
            public string? CreatedByName { get; set; }
            public string? CreatedIP { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTimeOffset? ModifiedDate { get; set; }
            public string? ModifiedByName { get; set; }
            public string? ModifiedIP { get; set; }
            public string? BillingAddress { get; set; }
            public string? DeliveryAddress { get; set; }
            public int? CostCenterId { get; set; }
            public decimal? FreightCharges { get; set; }
            public int? ModeOfDispatchId { get; set; }
            public int? TermsId { get; set; }
        }

        // Service Line
        public class ServiceLineDto
        {
            public int Id { get; set; }
            public int PurchaseOrderId { get; set; }
            public int ServicePoHeaderId { get; set; }
            public int LineNo { get; set; }
            public int? RequestId { get; set; }
            public int ServiceId { get; set; }
            public string ServiceDescription { get; set; } = string.Empty;
            public int UOMId { get; set; }
            public decimal PlannedQuantity { get; set; }
            public decimal PlannedRate { get; set; }
            public int? DiscountTypeId { get; set; }
            public decimal Discount { get; set; }
            public decimal ItemCost { get; set; }
            public decimal OtherCost { get; set; }
            public decimal OtherCharges { get; set; }
            public decimal GstPercent { get; set; }
            public string? Remarks { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
            public string? CreatedByName { get; set; }
            public string? CreatedIP { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTimeOffset? ModifiedDate { get; set; }
            public string? ModifiedByName { get; set; }
            public string? ModifiedIP { get; set; }
            public int? DiscountId { get; set; }
            public string? ServiceCode { get; set; }
            public decimal PlannedValue { get; set; }
        }

        // Service Schedule
        public class ServiceScheduleDto
        {
            public int Id { get; set; }
            public int PurchaseOrderId { get; set; }
            public int ServicePoHeaderId { get; set; }
            public int ServiceItemId { get; set; }
            public int ScheduleNo { get; set; }
            public string? OccurrencePeriod { get; set; }
            public string? OccurrenceDescription { get; set; }
            public int? ActivityTypeId { get; set; }
            public decimal? PlannedDurationHrs { get; set; }
            public DateTimeOffset? DueDate { get; set; }
            public DateTimeOffset? ServiceStartDate { get; set; }
            public DateTimeOffset? ServiceEndDate { get; set; }
            public decimal PlannedQuantity { get; set; }
            public decimal PlannedRate { get; set; }
            public decimal PlannedValue { get; set; }
            public bool AutoGenerateSES { get; set; }
            public string? Remarks { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTimeOffset CreatedDate { get; set; }
            public string? CreatedByName { get; set; }
            public string? CreatedIP { get; set; }
            public int? ModifiedBy { get; set; }
            public DateTimeOffset? ModifiedDate { get; set; }
            public string? ModifiedByName { get; set; }
            public string? ModifiedIP { get; set; }
        }
    }
}