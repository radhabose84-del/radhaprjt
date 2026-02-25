using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Domain.Entities
{
    public class SubLocation : BaseEntity
    {
        public string? Code { get; set; }
        public string? SubLocationName { get; set; }
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public int LocationId { get; set; }
        public Location? Location { get; set; } 
        //AssetSubLocation from AssetLocation
        public ICollection<AssetLocation>? AssetSubLocation{ get; set; } 
        public ICollection<AssetTransferReceiptDtl>? AssetTransferReceiptSubLocation{ get; set; } 



    }
}