using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using MediatR;

namespace FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchaseById
{
    public class GetAssetPurchaseByIdQuery : IRequest<AssetPurchaseDetailsDto>
    {
         public int Id { get; set; }
    }
}