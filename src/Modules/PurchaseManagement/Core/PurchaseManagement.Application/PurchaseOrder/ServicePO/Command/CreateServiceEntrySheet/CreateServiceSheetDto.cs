namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet
{
    public class CreateServiceSheetDto
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
        public decimal? TaxPercent { get; set; }
        public int? UOMId { get; set; }

        public int UnitId { get; set; }
        public string? AttachmentFileName { get; set; }
        public int ServiceId { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceDescription { get; set; }
        public int ScheduleID { get; set; }
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
        public string? ServicePOImage { get; set; }
        public string? ImageUrl { get; set; }

        // Child activities (collection)
        public List<CreateServiceEntryActivityDto> Activities { get; set; } = new();

        public List<ServiceEntryDocumentDto> ServiceEntryDocumentDtos { get; set; } = new();

        public class CreateServiceEntryActivityDto
        {
            public int Id { get; set; }
            public int? ActivityTypeId { get; set; }
            public string? Description { get; set; }
            public int PerformedById { get; set; }
            public string? PerformedByName { get; set; }
            public int? SESActivityStatusId { get; set; }
            public string? StatusRemarks { get; set; }
        }

        public class ServiceEntryDocumentDto
        {
            public int DocumentId { get; set; }          // from MiscMaster (DocumentType)
            public string FileName { get; set; } = "";   // temp_12345.pdf from UI
            public DateTimeOffset UploadedDate { get; set; }
        }

        // public class ServiceEntrySheetDocumentDto
        // {

        //     public int DocumentId { get; set; }
        //     public string? FileName { get; set; }
        //     public DateTimeOffset UploadedDate { get; set; }
        //     public string? UploadedPath { get; set; }
        //     public string? DocumentName { get; set; }  
     
        // }
        

    }
}