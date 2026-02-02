using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetPOById;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOById;

public sealed class GetCombinePOByIdQueryHandler : IRequestHandler<GetCombinePOByIdQuery, GetCombinePOByIdVm>
{
    private readonly IMediator _mediator;
    private readonly IPoMethodLookup _lookup; // resolves Local/Import IDs from MiscMaster

    public GetCombinePOByIdQueryHandler(IMediator mediator, IPoMethodLookup lookup)
    {
        _mediator = mediator;
        _lookup = lookup;
    }

    public async Task<GetCombinePOByIdVm> Handle(GetCombinePOByIdQuery request, CancellationToken ct)
    {
        var vm = new GetCombinePOByIdVm { POMethodId = request.POMethodId };

        if (await _lookup.IsLocalAsync(request.POMethodId, ct))
        {
            vm.Local = await _mediator.Send(new GetPurchaseOrderByIdQuery(request.Id), ct);
            if (vm.Local is null)
                throw new KeyNotFoundException($"Local PO {request.Id} not found.");
            return vm;
        }

        if (await _lookup.IsImportAsync(request.POMethodId, ct))
        {
            vm.Import = await _mediator.Send(new GetImportPOByIdQuery(request.Id), ct);
            if (vm.Import is null)
                throw new KeyNotFoundException($"Import PO {request.Id} not found.");
            return vm;
        }

        throw new InvalidOperationException("Unsupported POMethodId.");
    }
}
