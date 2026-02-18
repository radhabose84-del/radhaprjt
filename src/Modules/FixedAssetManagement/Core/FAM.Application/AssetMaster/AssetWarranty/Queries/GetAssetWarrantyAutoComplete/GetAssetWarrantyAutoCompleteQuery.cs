using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyAutoComplete
{
    public class GetAssetWarrantyAutoCompleteQuery : IRequest<List<AssetWarrantyAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}