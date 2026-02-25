using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO
{
    public class PurchaseOrderServiceSchedule  : BaseEntity
    {
        
        public int PurchaseOrderId { get; set; }
        public int ServicePoHeaderId { get; set; }
        public PurchaseOrderServiceHeader? ServicePoHeader { get; set; }
        public int ServiceItemId { get; set; }
        public int    ScheduleNo { get; set; }                
        public string? OccurrencePeriod { get; set; }
        public string?   OccurrenceDescription { get; set; }
        public int?     ActivityTypeId { get; set; }
        public decimal? PlannedDurationHrs { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate   { get; set; }
        public decimal? PlannedQuantity { get; set; }
        public decimal? PlannedRate     { get; set; }
        public decimal? PlannedValue   { get; set; }
        public bool AutoGenerateSES { get; set; }
        public string?  Remarks { get; set; }
        
    }
}