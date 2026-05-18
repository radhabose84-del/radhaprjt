using MediatR;
using PurchaseManagement.Application.PurchaseOrder.POAmendment;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Amendment;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Amendment;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Commands.AmendCombinePO;

public sealed class AmendCombinePOCommandHandler : IRequestHandler<AmendCombinePOCommand, int>
{
    private readonly IMediator _mediator;
    private readonly IPoMethodLookup _lookup;

    public AmendCombinePOCommandHandler(IMediator mediator, IPoMethodLookup lookup)
    { _mediator = mediator; _lookup = lookup; }

    public async Task<int> Handle(AmendCombinePOCommand request, CancellationToken ct)
    {
        var dto = request.Data;

        if (await _lookup.IsLocalAsync(dto.POMethodId, ct))
        {
            if (dto.Local is not null) dto.Local.POMethodId = dto.POMethodId;
            return await _mediator.Send(new POAmendmentCommand { Data = dto.Local! }, ct);
        }

        if (await _lookup.IsImportAsync(dto.POMethodId, ct))
        {
            if (dto.Import is not null) dto.Import.POMethodId = dto.POMethodId;
            return await _mediator.Send(new ImportPOAmendmentCommand { Data = dto.Import! }, ct);
        }

        if (await _lookup.IsContractAsync(dto.POMethodId, ct))
        {
            if (dto.Contract is not null) dto.Contract.POMethodId = dto.POMethodId;
            return await _mediator.Send(new ContractReleasePOAmendmentCommand { Data = dto.Contract! }, ct);
        }

        throw new InvalidOperationException("Unsupported POMethodId.");
    }
}
