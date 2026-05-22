using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal
{
    public class CreateAssetDisposalCommand :IRequest<int>, IRequirePermission
    {
        public int AssetId { get; set; } 
        public int AssetPurchaseId { get; set; } 
        public DateOnly DisposalDate { get; set; }
        public int? DisposalType { get; set; }  
        public string? DisposalReason { get; set; }
        public decimal? DisposalAmount { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
