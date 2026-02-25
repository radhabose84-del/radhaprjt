using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetParentMaster
{
    public class GetAssetParentMasterQuery : IRequest<List<AssetMasterGeneralAutoCompleteDTO>>
    {
        public string? AssetType { get; set; }
    }
}