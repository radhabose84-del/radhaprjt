using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DispatchAddressMapping.Commands.DeleteDispatchAddressMapping
{
    public sealed record DeleteDispatchAddressMappingCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
