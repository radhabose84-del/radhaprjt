using FAM.Domain.Common;

namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetSpecifications : BaseEntity
    {
         public int AssetId { get; set; } 
         public AssetMasterGenerals AssetMasterId { get; set; } = null!;
         public int SpecificationId { get; set; } 
         public SpecificationMasters SpecificationMaster { get; set; } = null!;
         public string? SpecificationValue { get; set; }          
    }
}