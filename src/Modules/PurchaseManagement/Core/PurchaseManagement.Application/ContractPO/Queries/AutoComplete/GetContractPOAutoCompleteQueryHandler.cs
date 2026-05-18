using Contracts.Interfaces.Lookups.Party;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IContractPO;
using PurchaseManagement.Application.ContractPO.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.ContractPO.Queries.AutoComplete;

public sealed class GetContractPOAutoCompleteQueryHandler
    : IRequestHandler<GetContractPOAutoCompleteQuery, IReadOnlyList<ContractPOLookupDto>>
{
    private readonly IContractPOQueryRepository _repo;
    private readonly IMediator _mediator;
    private readonly IPartyLookup _partyLookup;

    public GetContractPOAutoCompleteQueryHandler(
        IContractPOQueryRepository repo,
        IMediator mediator,
        IPartyLookup partyLookup)
    {
        _repo = repo;
        _mediator = mediator;
        _partyLookup = partyLookup;
    }

    public async Task<IReadOnlyList<ContractPOLookupDto>> Handle(
        GetContractPOAutoCompleteQuery request, CancellationToken ct)
    {
        var results = await _repo.AutocompleteAsync(request.Term ?? string.Empty, ct);
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
            actionCode: "GetContractPOAutoCompleteQuery",
            actionName: items.Count.ToString(),
            details: "Contract PO autocomplete fetched.",
            module: "ContractPO"
        );
        await _mediator.Publish(ev, ct);

        return items;
    }
}
