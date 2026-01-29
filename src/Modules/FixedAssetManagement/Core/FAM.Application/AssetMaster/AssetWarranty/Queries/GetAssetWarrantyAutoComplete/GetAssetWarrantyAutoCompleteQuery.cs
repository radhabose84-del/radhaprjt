using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyAutoComplete
{
    public class GetAssetWarrantyAutoCompleteQuery : IRequest<List<AssetWarrantyAutoCompleteDTO>>
    {
        public string? SearchPattern { get; set; }
    }
}