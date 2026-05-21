using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Amendment;

public sealed class ContractPOAmendmentCommand : IRequest<int>
{
    public ContractPOUpdateDto Data { get; set; } = default!;
}
