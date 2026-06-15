using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost
{
    public class UpdateAssetAdditionalCostCommand  :IRequest<int>, IRequirePermission
    { 
        public int Id {get; set;}
        public decimal Amount { get; set; }
        public string? JournalNo { get; set; }
        public int? CostType { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
