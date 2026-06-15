using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader
{
    public sealed record DeleteEWaybillHeaderCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
