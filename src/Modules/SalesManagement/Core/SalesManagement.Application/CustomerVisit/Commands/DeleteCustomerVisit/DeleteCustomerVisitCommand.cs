using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisit
{
    public sealed record DeleteCustomerVisitCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
