using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal
{
    public class UpdateAssetDisposalCommand :IRequest<int>, IRequirePermission
    {
        public int Id { get; set; } 
        public DateOnly DisposalDate { get; set; }
        public int? DisposalType { get; set; }  
        public string? DisposalReason { get; set; }
        public decimal? DisposalAmount { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
