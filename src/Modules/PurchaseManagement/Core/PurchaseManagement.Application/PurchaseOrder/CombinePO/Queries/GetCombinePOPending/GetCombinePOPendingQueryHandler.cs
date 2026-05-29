using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Queries.GetPending;
using PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetContractPOPending;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOPending;

public sealed class GetCombinePOPendingQueryHandler
    : IRequestHandler<GetCombinePOPendingQuery, GetCombinePOPendingVm>
{
    private readonly IMediator _mediator;
    private readonly IPoMethodLookup _lookup;

    public GetCombinePOPendingQueryHandler(IMediator mediator, IPoMethodLookup lookup)
    {
        _mediator = mediator;
        _lookup = lookup;
    }

    public async Task<GetCombinePOPendingVm> Handle(
        GetCombinePOPendingQuery request, CancellationToken ct)
    {
        var vm = new GetCombinePOPendingVm { POMethodId = request.PoMethodId };

        // If a specific POMethodId is provided, route to that handler only
        if (request.PoMethodId.HasValue)
        {
            if (await _lookup.IsLocalAsync(request.PoMethodId.Value, ct))
            {
                var (items, total) = await _mediator.Send(new GetPOLocalPendingQuery
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    PoId = request.PoId,
                    PoMethodId = request.PoMethodId
                }, ct);
                vm.LocalItems = items;
                vm.LocalTotalCount = total;
                return vm;
            }

            if (await _lookup.IsImportAsync(request.PoMethodId.Value, ct))
            {
                var (items, total) = await _mediator.Send(new GetImportPOsPendingQuery
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    PoId = request.PoId
                }, ct);
                vm.ImportItems = items;
                vm.ImportTotalCount = total;
                return vm;
            }

            if (await _lookup.IsContractAsync(request.PoMethodId.Value, ct))
            {
                var (items, total) = await _mediator.Send(new GetContractPOPendingQuery
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    PoId = request.PoId
                }, ct);
                vm.ContractItems = items;
                vm.ContractTotalCount = total;
                return vm;
            }

            if (await _lookup.IsBlanketAsync(request.PoMethodId.Value, ct))
            {
                var (items, total) = await _mediator.Send(new GetBlanketPOPendingQuery
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    PoId = request.PoId
                }, ct);
                vm.BlanketItems = items;
                vm.BlanketTotalCount = total;
                return vm;
            }

            throw new InvalidOperationException("Unsupported POMethodId.");
        }

        // No POMethodId → fetch all three sequentially
        // (IDbConnection is scoped — concurrent queries on the same connection are not supported)
        var (localItems, localTotal) = await _mediator.Send(new GetPOLocalPendingQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            PoId = request.PoId
        }, ct);
        vm.LocalItems = localItems;
        vm.LocalTotalCount = localTotal;

        var (importItems, importTotal) = await _mediator.Send(new GetImportPOsPendingQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            PoId = request.PoId
        }, ct);
        vm.ImportItems = importItems;
        vm.ImportTotalCount = importTotal;

        var (contractItems, contractTotal) = await _mediator.Send(new GetContractPOPendingQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            PoId = request.PoId
        }, ct);
        vm.ContractItems = contractItems;
        vm.ContractTotalCount = contractTotal;

        var (blanketItems, blanketTotal) = await _mediator.Send(new GetBlanketPOPendingQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            PoId = request.PoId
        }, ct);
        vm.BlanketItems = blanketItems;
        vm.BlanketTotalCount = blanketTotal;

        return vm;
    }
}
