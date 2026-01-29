using FAM.Application.Common.Mappings;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty
{
    public class AssetWarrantyAutoCompleteDTO : IMapFrom<AssetWarranties>
    {
        public int Id { get; set; }        
        public string? AssetCode { get; set; } 
    }
}