using FAM.Application.Common.Mappings;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification
{
    public class AssetSpecificationAutoCompleteDTO : IMapFrom<AssetSpecifications>
    {
        public int Id { get; set; }        
        public string? SpecificationName { get; set; } 
    }
}