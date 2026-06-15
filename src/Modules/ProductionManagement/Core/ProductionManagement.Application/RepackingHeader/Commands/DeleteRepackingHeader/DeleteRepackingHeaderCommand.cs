using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader
{
    public sealed record DeleteRepackingHeaderCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
