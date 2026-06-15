using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.ContractPOMaster.Commands.Delete;

public sealed record DeleteContractPOMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
