using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost
{
    public class CreateAssetAdditionalCostCommand :IRequest<int>, IRequirePermission
    {
        public int AssetId { get; set; }   
        public int AssetSourceId { get; set; }   
        public decimal Amount { get; set; }
        public string? JournalNo { get; set; }
        public int? CostType { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
