using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Update;

public sealed record UpdateContractReleasePOCommand(ContractReleasePOUpdateDto Data) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}
