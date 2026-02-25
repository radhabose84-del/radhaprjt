using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete
{
    public class GetAssetSourceAutoCompleteQuery  : IRequest<List<AssetSourceAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}