using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Delete;

public sealed record DeleteContractReleasePOCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
