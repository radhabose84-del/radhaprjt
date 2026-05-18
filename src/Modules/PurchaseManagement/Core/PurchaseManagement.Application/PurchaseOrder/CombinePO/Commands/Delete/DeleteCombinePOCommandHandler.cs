using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Delete;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Delete;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Delete;

public sealed class DeleteCombinePOCommandHandler
    : IRequestHandler<DeleteCombinePOCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly IPoMethodLookup _lookup;

    public DeleteCombinePOCommandHandler(IMediator mediator, IPoMethodLookup lookup)
    {
        _mediator = mediator;
        _lookup = lookup;
    }

    public async Task<bool> Handle(DeleteCombinePOCommand request, CancellationToken ct)
    {
        if (await _lookup.IsLocalAsync(request.POMethodId, ct))
            return await _mediator.Send(new DeletePurchaseOrderCommand { Id = request.Id }, ct);

        if (await _lookup.IsContractAsync(request.POMethodId, ct))
            return await _mediator.Send(new DeleteContractReleasePOCommand(request.Id), ct);

        throw new InvalidOperationException("Unsupported POMethodId for delete.");
    }
}
