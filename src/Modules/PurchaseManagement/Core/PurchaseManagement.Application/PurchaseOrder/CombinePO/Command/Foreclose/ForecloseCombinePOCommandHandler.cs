using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Foreclose;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Foreclose;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Foreclose;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Foreclose;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Foreclose;

public sealed class ForecloseCombinePOCommandHandler
    : IRequestHandler<ForecloseCombinePOCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly IPoMethodLookup _lookup;

    public ForecloseCombinePOCommandHandler(IMediator mediator, IPoMethodLookup lookup)
    {
        _mediator = mediator;
        _lookup = lookup;
    }

    public async Task<bool> Handle(ForecloseCombinePOCommand request, CancellationToken ct)
    {
        if (await _lookup.IsLocalAsync(request.POMethodId, ct))
            return await _mediator.Send(new ForeclosePurchaseOrderCommand(request.Id), ct);

        if (await _lookup.IsImportAsync(request.POMethodId, ct))
            return await _mediator.Send(new ForecloseImportPOCommand(request.Id), ct);

        if (await _lookup.IsContractAsync(request.POMethodId, ct))
            return await _mediator.Send(new ForecloseContractPOCommand(request.Id), ct);

        if (await _lookup.IsBlanketAsync(request.POMethodId, ct))
            return await _mediator.Send(new ForecloseBlanketPOCommand(request.Id), ct);

        throw new InvalidOperationException("Unsupported POMethodId for foreclose.");
    }
}
