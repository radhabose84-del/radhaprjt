using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO
{

    public class ServicePurchaseOrderUpdateDto : CreateServicePurchaseOrderDto
    {
        public int Id { get; set; }
    }
    public class CreateServicePurchaseOrderDto
    {

        // 1. Main PO header (common)
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string PONumber { get; set; } = default!;
        public DateTimeOffset PODate { get; set; }
        public int POCategoryId { get; set; }
        public string? POCategory { get; set; }
        public int? POMethodId { get; set; }
        public string? POMethod { get; set; }
        public int CurrencyId { get; set; }
        public string? Currency { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }

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
        public decimal PurchaseValue { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public int RevisionNo { get; set; }
        public string? AmendmentReason { get; set; }
        public int Edit { get; set; }
        public string? EditReason { get; set; }


        public List<PurchaseOrderServiceHeaderDto> ServicePos { get; set; } = new();

        public List<PurchaseOrderServicePaymentTermDto> PaymentTerms { get; set; } = new();

        public List<PurchaseServiceDocumentDto> Documents { get; set; } = new();
    }

    public sealed class PurchaseOrderServiceDetailDto
    {

        public int Id { get; set; }
        public int UnitId { get; set; }
        public string PONumber { get; set; } = default!;
        public DateTimeOffset PODate { get; set; }
        public int POCategoryId { get; set; }
        public string? POCategory { get; set; }
        public int? POMethodId { get; set; }
        public string? POMethod { get; set; }
        public int CurrencyId { get; set; }
        public string? Currency { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }

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
        public decimal PurchaseValue { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public int RevisionNo { get; set; }
        public string? AmendmentReason { get; set; }
        public int Edit { get; set; }
        public string? EditReason { get; set; }
        public new List<PurchaseOrderServiceHeaderDto> ServicePo { get; set; } = new();
        public new List<PurchaseOrderServicePaymentTermDto> PaymentTerms { get; set; } = new();
        public new List<ServiceDocumentDto> DocumentsList { get; set; } = new();
    }

    public sealed class PurchaseOrderServiceHeaderDto
    {
        public int? Id { get; set; }
        public int PurchaseOrderId { get; set; }   // handler will set
        public int ServiceCategoryId { get; set; }
        public string? ServiceCategory { get; set; }
        public int? ContractTypeId { get; set; }
        public string? ContractType { get; set; }
        public int? FrequencyId { get; set; }
        public string? Frequency { get; set; }
        public DateTimeOffset? ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }
        public int? TotalOccurrences { get; set; }
        public decimal? OverallLimit { get; set; }
        public int? TermsId { get; set; }
        public string? TermDescription { get; set; }
        public int? CostCenterId { get; set; }
        public string? CostCenter { get; set; }
        public int? ModeOfDispatchId { get; set; }
        public decimal? FreightCharges { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? BillingAddress { get; set; }
        public string? POImage { get; set; }
        public string? ImageUrl { get; set; }

        // 👇 nested
        public List<PurchaseOrderServiceLineDto> Lines { get; set; } = new();
    }
    public sealed class PurchaseOrderServicePaymentTermDto
    {
        public int? Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int PaymentTermId { get; set; }
        public string? PaymentTerm { get; set; }

        public decimal? AdvancePercent { get; set; }
        public int? CreditDays { get; set; }
        public int? PaymentModelId { get; set; }
        public string? PaymentModel { get; set; }
        public int? InsuranceId { get; set; }
        public string? Insurance { get; set; }
        public int? InsurancePercent { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? BalancePercent { get; set; }
        public decimal? BalanceAmount { get; set; }
    }

    public sealed class PurchaseOrderServiceLineDto
    {
        public int? Id { get; set; }
        public int PurchaseOrderId { get; set; }      // handler will set
        public int ServicePoHeaderId { get; set; }    // handler will set
        public int LineNo { get; set; }
        public int? RequestId { get; set; }
        public string? Request { get; set; }
        public int? ServiceId { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceDescription { get; set; }
        public int? UOMId { get; set; }
        public string? UOM { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal PlannedRate { get; set; }
        public decimal PlannedValue { get; set; }
        public int? DiscountId { get; set; }
        public string? DiscountType { get; set; }
        public decimal? Discount { get; set; }
        public decimal? ItemCost { get; set; }
        public decimal? OtherCost { get; set; }
        public decimal? OtherCharges { get; set; }
        public decimal? GstPercent { get; set; }
        public string? Remarks { get; set; }

        public List<PurchaseOrderServiceScheduleDto> Schedules { get; set; } = new();
    }

    public sealed class PurchaseOrderServiceScheduleDto
    {
        public int? Id { get; set; }
        public int PurchaseOrderId { get; set; }    // handler will set
        public int ServicePoHeaderId { get; set; }  // handler will set
        public int ServiceItemId { get; set; }      // handler will set = line.Id
        public string? ServiceName { get; set; }
        public int ScheduleNo { get; set; }
        public string? OccurrencePeriod { get; set; }
        public string? OccurrenceDescription { get; set; }
        public int? ActivityTypeId { get; set; }
        public string? ActivityType { get; set; }
        public decimal? PlannedDurationHrs { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public decimal? PlannedQuantity { get; set; }
        public decimal? PlannedRate { get; set; }
        public decimal? PlannedValue { get; set; }
        public bool AutoGenerateSES { get; set; }
        public string? Remarks { get; set; }
    }
    public sealed class PurchaseServiceDocumentDto
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = "";
        public DateTimeOffset UploadedDate { get; set; }
    }
    
     public class ServiceDocumentDto
    {
        public int DocumentId { get; set; }
        public string? FileName { get; set; }
        public DateTimeOffset UploadedDate { get; set; }
        public string? UploadedPath { get; set; }          
        public string? DocumentName { get; set; }  
       }
  

    

}