using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete
{
    public class GetAssetUnitAutoCompleteQuery : IRequest<List<AssetUnitAutoCompleteDto>>
    {
        public string? Username { get; set; }
    }
}