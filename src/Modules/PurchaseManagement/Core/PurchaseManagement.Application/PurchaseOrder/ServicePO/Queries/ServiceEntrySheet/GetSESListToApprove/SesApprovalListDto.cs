namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetSESListToApprove
{
    public class SesApprovalListDto
    {
         public int Id { get; set; }
        public DateTimeOffset SESDate { get; set; }
        public int PurchaseOrderId { get; set; }
        public DateTimeOffset PODate { get; set; }
        public int VendorId { get; set; }
        public string? VendorName { get; set; }    // if you join vendor later
        public string? ServiceDescription { get; set; }
        public int ScheduleId { get; set; }
        public int OccurrenceNo { get; set; }
        public string? OccurrencePeriod { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal ActualRate { get; set; }
        public decimal ActualValue { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxValue { get; set; }
        public decimal TotalValue { get; set; }
        public string? LineRemarks { get; set; }

        public int SESStatusId { get; set; }       // Approval status (Pending / Approved)
        public int StatusId { get; set; }          // Workflow status if you use it
        public int UnitId { get; set; }

        public string? CreatedByName { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}