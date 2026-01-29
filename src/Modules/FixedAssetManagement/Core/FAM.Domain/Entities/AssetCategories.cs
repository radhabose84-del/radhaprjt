using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class AssetCategories :BaseEntity
    {
        public string? Code { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int AssetGroupId { get; set; }
        public AssetGroup AssetGroup { get; set; } = null!;
        public ICollection<AssetSubCategories>? AssetSubCategories { get; set; } 
        public ICollection<AssetMasterGenerals>? AssetMasterGeneral { get; set; } 

    }
}