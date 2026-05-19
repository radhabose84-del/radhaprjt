using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Cancel;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Cancel;

public sealed class CancelCombinePOCommandHandler
    : IRequestHandler<CancelCombinePOCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly IPoMethodLookup _lookup;

    public CancelCombinePOCommandHandler(IMediator mediator, IPoMethodLookup lookup)
    {
        _mediator = mediator;
        _lookup = lookup;
    }

    public async Task<bool> Handle(CancelCombinePOCommand request, CancellationToken ct)
    {
        if (await _lookup.IsLocalAsync(request.POMethodId, ct))
            return await _mediator.Send(new CancelPurchaseOrderCommand(request.Id), ct);

        if (await _lookup.IsImportAsync(request.POMethodId, ct))
            return await _mediator.Send(new CancelPurchaseOrderCommand(request.Id), ct);

        if (await _lookup.IsContractAsync(request.POMethodId, ct))
            return await _mediator.Send(new CancelPurchaseOrderCommand(request.Id), ct);

        throw new InvalidOperationException("Unsupported POMethodId for cancel.");
    }
}
