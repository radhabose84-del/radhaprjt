using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrganisation.Commands.DeleteSalesOrganisation;

public sealed record DeleteSalesOrganisationCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
