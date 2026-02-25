using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCostById
{
    public class GetAssetAdditionalCostByIdQuery : IRequest<AssetAdditionalCostDto>
    {
        public int Id { get; set; }
    }
}