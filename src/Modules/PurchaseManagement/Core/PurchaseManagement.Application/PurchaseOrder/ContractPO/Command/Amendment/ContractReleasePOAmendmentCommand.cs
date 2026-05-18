using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Amendment;

public sealed class ContractReleasePOAmendmentCommand : IRequest<int>
{
    public ContractReleasePOUpdateDto Data { get; set; } = default!;
}
