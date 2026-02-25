using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO
{
    public class ServiceEntrySheet : BaseEntity
    {
        public DateTimeOffset SESDate { get; set; }
        public int SESStatusId { get; set; }
        public int PurchaseOrderId { get; set; }
        // <— add this
        // Foreign key navigation properties
        public DateTimeOffset PODate { get; set; }
        public int VendorId { get; set; }
        public int? ServiceCategoryId { get; set; } // Foreign key to MiscMaster (Service Category)
        public int? ContractTypeId { get; set; }
        // Other SES fields
        public DateTimeOffset? ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }
        public int UnitId { get; set; }
        public string? AttachmentFileName { get; set; }
        public int ServiceId { get; set; }
        public string? ServiceDescription { get; set; }
        public int ScheduleId { get; set; }
        public int OccurrenceNo { get; set; }
        public string? OccurrencePeriod { get; set; }
        public DateTimeOffset ScheduleStartDate { get; set; }
        public DateTimeOffset ScheduleEndDate { get; set; }
        public decimal? ActualQuantity { get; set; }
        public decimal? ActualRate { get; set; }
        public decimal? ActualValue { get; set; }
        public int? DiscountTypeId { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? TaxValue { get; set; }
        public decimal? TotalValue { get; set; }
        public DateTimeOffset? WorkStartDate { get; set; }
        public DateTimeOffset? WorkEndDate { get; set; }
        public decimal? DurationHrs { get; set; }
        public string? LineRemarks { get; set; }

        public int StatusId { get; set; }


        // Navigation properties to related entities
        public PurchaseOrderHeader? PurchaseOrder { get; set; } // Foreign key to PurchaseOrderHeader
        public MiscMaster? ServiceCategory { get; set; } // Navigation to MiscMaster for Service Category
        public MiscMaster? ContractType { get; set; } //
        public MiscMaster? DiscountType { get; set; }
        public MiscMaster? SESStatus { get; set; }


        public ICollection<ServiceEntryActivity> Activities { get; set; } = new List<ServiceEntryActivity>();        
         
        public ICollection<ServiceEntrySheetDocument> Documents { get; set; }      = new List<ServiceEntrySheetDocument>();


    }
}