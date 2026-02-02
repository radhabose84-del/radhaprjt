using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES
{
    public class ServiceEntrySheetWithActivitiesDto
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
        public string? ServiceCode { get; set; }
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

        // 👇 Activities for this SES
        public List<GetAllServiceEntryActivityDto> Activities { get; set; } = new();
        
         public List<ServiceEntrySheetDocumentDto> Documents { get; set; } = new();
    }
}