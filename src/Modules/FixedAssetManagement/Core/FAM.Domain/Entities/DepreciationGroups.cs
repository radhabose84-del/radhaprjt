using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class DepreciationGroups : BaseEntity
    {
        public string? Code { get; set; }
        public string? DepreciationGroupName { get; set; }       
        public int? BookType { get; set; }        
        public MiscMaster BookMiscType { get; set; } = null!;                 
        // Foreign Key
        public int AssetGroupId { get; set; }
        public AssetGroup AssetGroup { get; set; } = null!;
        public decimal UsefulLife { get; set; }
        public int? DepreciationMethod { get; set; }        
        public MiscMaster DepMiscType { get; set; } = null!;     
        public int? ResidualValue { get; set; }
        public int SortOrder { get; set; }
        public decimal DepreciationRate { get; set; }
    }
}