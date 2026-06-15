using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.CommissionSplit.Commands.UpdateCommissionSplit
{
    public class UpdateCommissionSplitCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SplitName { get; set; }
        public int IsActive { get; set; }
        public List<CreateCommissionSplit.CommissionSplitDetailItem>? Details { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
