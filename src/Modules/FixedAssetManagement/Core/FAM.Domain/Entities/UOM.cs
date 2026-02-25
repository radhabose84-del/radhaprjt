using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class UOM : BaseEntity
    {
        public string? Code { get; set; }
        public string? UOMName { get; set; }
        public int UOMTypeId { get; set; } // Foreign Key to MiscMaster
        public int SortOrder { get; set; }
       
       // Foreign Key relationship with MiscMaster
        public MiscMaster UOMType { get; set; } = null!;
        public ICollection<AssetMasterGenerals>? AssetGeneralsUom  { get; set; }  

    }
}