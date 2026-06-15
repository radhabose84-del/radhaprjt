using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesChannel.Commands.DeleteSalesChannel;

public sealed record DeleteSalesChannelCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
