using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Update;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.Update;

public sealed class UpdateCombinePOCommandHandler : IRequestHandler<UpdateCombinePOCommand, bool>
{
    private readonly IMediator _mediator;
    private readonly IPoMethodLookup _lookup;

    public UpdateCombinePOCommandHandler(IMediator mediator, IPoMethodLookup lookup)
    { _mediator = mediator; _lookup = lookup; }

    public async Task<bool> Handle(UpdateCombinePOCommand request, CancellationToken ct)
    {
        var dto = request.Data;

        if (await _lookup.IsLocalAsync(dto.POMethodId, ct))
        {
            if (dto.Local is not null) dto.Local.POMethodId = dto.POMethodId;
            return await _mediator.Send(new UpdatePurchaseOrderCommand { Data = dto.Local! }, ct);
        }

        if (await _lookup.IsImportAsync(dto.POMethodId, ct))
        {
            if (dto.Import is not null) dto.Import.POMethodId = dto.POMethodId;
            return await _mediator.Send(new UpdateImportPOCommand { Data = dto.Import! }, ct);
        }

        if (await _lookup.IsContractAsync(dto.POMethodId, ct))
        {
            if (dto.Contract is not null) dto.Contract.POMethodId = dto.POMethodId;
            return await _mediator.Send(new UpdateContractPOCommand(dto.Contract!), ct);
        }

        throw new InvalidOperationException("Unsupported POMethodId.");
    }
}
