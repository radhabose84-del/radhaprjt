using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PriceMaster
{
    public class PriceMasterDetail : BaseEntity, IActivityTracked
    {
        public int PriceMasterHeaderId { get; set; }
        public PriceMasterHeader PriceMasterHeader { get; set; } = null!;
        public decimal ScaleQtyFrom { get; set; }
        public decimal? ScaleQtyTo { get; set; }
        public decimal UnitPrice { get; set; }   
        public int CurrencyId { get; set; }     
    }
}