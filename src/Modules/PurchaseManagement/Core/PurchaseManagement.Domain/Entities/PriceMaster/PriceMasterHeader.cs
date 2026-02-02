using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PriceMaster
{
    public class PriceMasterHeader : BaseEntity,IActivityTracked
    {
        public int UnitId { get; set; }
        public int ItemId { get; set; }
        public int VendorId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }        
        public int StatusId { get; set; }
        public MiscMaster? MiscStatus { get; set; }
        public int SourceFromId { get; set; }
        public MiscMaster MiscSourceFrom { get; set; } = default!;
        public int? SourceDetailId { get; set; }     
        public int UomId { get; set; }
        public ICollection<PriceMasterDetail> Details { get; set; } = new List<PriceMasterDetail>();
    }
}