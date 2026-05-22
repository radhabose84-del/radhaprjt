using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.CommissionSplit.Commands.DeleteCommissionSplit
{
    public sealed record DeleteCommissionSplitCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
