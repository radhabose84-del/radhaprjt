using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class AssetSubGroup : BaseEntity
    {
        public string? Code { get; set; }
        public string? SubGroupName { get; set; }
        public int SortOrder { get; set; }
        public int GroupId { get; set; }
        public decimal SubGroupPercentage { get; set; }
        public byte AdditionalDepreciation { get; set; }
        public AssetGroup AssetGroup { get; set; } = null!;
        public ICollection<WDVDepreciationDetail>? WDVDepreciationDetail { get; set; }
        public ICollection<AssetMasterGenerals>? AssetMasterGeneral { get; set; }
    }
}
