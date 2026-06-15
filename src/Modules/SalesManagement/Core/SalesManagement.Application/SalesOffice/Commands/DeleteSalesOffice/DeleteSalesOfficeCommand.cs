using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOffice.Commands.DeleteSalesOffice
{
    public sealed record DeleteSalesOfficeCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
