

using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class AssetMasterGeneralAutoCompleteDTO : IMapFrom<AssetMasterGenerals>
    {
        public int Id { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; } 
    }
}