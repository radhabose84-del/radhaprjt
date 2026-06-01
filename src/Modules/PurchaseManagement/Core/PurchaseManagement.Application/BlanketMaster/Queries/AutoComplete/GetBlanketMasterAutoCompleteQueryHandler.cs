using Contracts.Interfaces.Lookups.Party;
using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BlanketMaster.Queries.AutoComplete;

public sealed class GetBlanketMasterAutoCompleteQueryHandler
    : IRequestHandler<GetBlanketMasterAutoCompleteQuery, IReadOnlyList<BlanketMasterLookupDto>>
{
    private readonly IBlanketMasterQueryRepository _repo;
    private readonly IMediator _mediator;
    private readonly IPartyLookup _partyLookup;

    public GetBlanketMasterAutoCompleteQueryHandler(
        IBlanketMasterQueryRepository repo,
        IMediator mediator,
        IPartyLookup partyLookup)
    {
        _repo = repo;
        _mediator = mediator;
        _partyLookup = partyLookup;
    }

    public async Task<IReadOnlyList<BlanketMasterLookupDto>> Handle(
        GetBlanketMasterAutoCompleteQuery request, CancellationToken ct)
    {
        var results = await _repo.AutocompleteAsync(
            request.Term ?? string.Empty, request.ApprovedOnly,
            request.VendorId, request.PODate ?? DateTimeOffset.UtcNow, ct);
        var items = results.ToList();

        // Enrich: Vendor names
        if (items.Count > 0)
        {
            var vendorIds = items.Select(x => x.VendorId).Where(x => x > 0).Distinct().ToArray();
            if (vendorIds.Length > 0)
            {
                var vendors = await _partyLookup.GetByIdsAsync(vendorIds, ct);
                var vendorDict = vendors.ToDictionary(v => v.Id, v => v.PartyName);
                foreach (var item in items)
                {
                    if (vendorDict.TryGetValue(item.VendorId, out var name))
                        item.VendorName = name;
                }
            }
        }

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetAll",
            actionCode: "GetBlanketMasterAutoCompleteQuery",
            actionName: items.Count.ToString(),
            details: "Blanket Master autocomplete fetched.",
            module: "BlanketMaster"
        );
        await _mediator.Publish(ev, ct);

        return items;
    }
}
