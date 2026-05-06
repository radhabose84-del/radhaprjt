namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid
{
    public class SesFromScheduleRawDto
    {
        public int Id { get; set; }      

        // Header
        public int PurchaseOrderId { get; set; }
        public int UnitId { get; set; }
        public DateTimeOffset PODate { get; set; }
        public int VendorId { get; set; }

        // PO Service Header
        public int? ServiceCategoryId { get; set; }
        public int? ContractTypeId { get; set; }
        public DateTimeOffset? ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }

        // Line
        public decimal? PlannedQuantity { get; set; }
        public decimal? PlannedRate { get; set; }
        public decimal? PlannedValue { get; set; }
        public decimal? GstPercent { get; set; }
        public int? UOMId { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }   // c.Discount AS DiscountValue
        public int LineNumber { get; set; }           // c.[LineNo] AS LineNumber
        public int ServiceId { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceDescription { get; set; }

        // Schedule
        public int ScheduleId { get; set; }           // d.Id AS ScheduleId
        public int OccurrenceNo { get; set; }         // d.ScheduleNo AS OccurrenceNo
        public string? OccurrencePeriod { get; set; }
        public DateTime? ScheduleStartDate { get; set; }       // d.ServiceStartDate (datetime2)
        public DateTime? ScheduleEndDate { get; set; }         // d.ServiceEndDate (datetime2)
        public bool AutoGenerateSES { get; set; }
        public string? LineRemarks { get; set; } 
        public int StatusId { get; set; }


    //      public List<ServiceEntryActivitiesDto> Activities { get; set; } = new();
    //     public class ServiceEntryActivitiesDto
    // {
    //     public int Id { get; set; }
    //     public int? ActivityTypeId { get; set; }
    //     public string? Description { get; set; }
    //     public int PerformedById { get; set; }
    //     public string? PerformedByName { get; set; }
    //     public int? SESActivityStatusId { get; set; }
    //     public string? StatusRemarks { get; set; }
    // }


    }
}