
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralAutoComplete
{
    public class GetAssetMasterGeneralAutoCompleteQuery  : IRequest<List<AssetMasterGeneralAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}