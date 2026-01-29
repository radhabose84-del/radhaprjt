using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class AssetGroup : BaseEntity
    {
        public string? Code { get; set; }
        public string? GroupName { get; set; }
        public int SortOrder { get; set; }
        public decimal GroupPercentage { get; set; }
        public ICollection<DepreciationGroups>? DepreciationGroups { get; set; }
        public ICollection<AssetCategories>? AssetCategories { get; set; }
        public ICollection<AssetMasterGenerals>? AssetMasterGeneral { get; set; }
        public ICollection<SpecificationMasters>? SpecificationMaster { get; set; }
        public ICollection<DepreciationDetails>? DepreciationDetails { get; set; }
        public ICollection<AssetSubGroup>? AssetSubGroup { get; set; } 
        public ICollection<WDVDepreciationDetail>? WDVDepreciationDetail { get; set; }
    }
}
