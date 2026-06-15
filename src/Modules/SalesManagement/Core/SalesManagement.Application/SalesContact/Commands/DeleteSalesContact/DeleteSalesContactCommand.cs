using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesContact.Commands.DeleteSalesContact
{
    public sealed record DeleteSalesContactCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
