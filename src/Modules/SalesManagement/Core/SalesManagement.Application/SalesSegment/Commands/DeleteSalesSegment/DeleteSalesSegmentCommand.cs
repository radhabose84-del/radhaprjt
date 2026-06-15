
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesSegment.Commands.DeleteSalesSegment
{
    public sealed record DeleteSalesSegmentCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
