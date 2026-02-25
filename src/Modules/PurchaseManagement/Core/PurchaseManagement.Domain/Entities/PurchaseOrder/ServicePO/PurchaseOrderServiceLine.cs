using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO
{
    public class PurchaseOrderServiceLine  : BaseEntity
    {

        public int PurchaseOrderId { get; set; }   // ties back to the PO/ServiceOrderHeader
        public int ServicePoHeaderId { get; set; }
         public PurchaseOrderServiceHeader? ServicePoHeader { get; set; }  
        public int LineNo { get; set; }
        public int?  RequestId { get; set; }
        public int? ServiceId { get; set; }   
        public string? ServiceCode { get; set; }     
        public string? ServiceDescription { get; set; }        
        public int? UOMId { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal PlannedRate     { get; set; }
        public decimal PlannedValue    { get; set; }
        public int? DiscountId { get; set; }     
        public decimal? Discount { get; set; }
        public decimal? ItemCost     { get; set; }
        public decimal? OtherCost    { get; set; }
        public decimal? OtherCharges { get; set; }
        public decimal? GstPercent { get; set; }
        public string?  Remarks    { get; set; }
        // Navigations
        public virtual ICollection<PurchaseOrderServiceSchedule> PurchaseOrderServiceSchedules { get; set; } = new List<PurchaseOrderServiceSchedule>();
        
    }
}