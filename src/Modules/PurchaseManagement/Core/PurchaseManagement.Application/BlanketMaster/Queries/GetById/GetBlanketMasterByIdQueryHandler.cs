using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BlanketMaster.Queries.GetById;

public sealed class GetBlanketMasterByIdQueryHandler
    : IRequestHandler<GetBlanketMasterByIdQuery, BlanketHeaderDto?>
{
    private readonly IBlanketMasterQueryRepository _repo;
    private readonly IMediator _mediator;
    private readonly IPartyLookup _partyLookup;
    private readonly ICurrencyLookup _currencyLookup;
    private readonly IItemLookup _itemLookup;

    public GetBlanketMasterByIdQueryHandler(
        IBlanketMasterQueryRepository repo,
        IMediator mediator,
        IPartyLookup partyLookup,
        ICurrencyLookup currencyLookup,
        IItemLookup itemLookup)
    {
        _repo = repo;
        _mediator = mediator;
        _partyLookup = partyLookup;
        _currencyLookup = currencyLookup;
        _itemLookup = itemLookup;
    }

    public async Task<BlanketHeaderDto?> Handle(
        GetBlanketMasterByIdQuery request, CancellationToken ct)
    {
        var dto = await _repo.GetByIdAsync(request.Id, ct);
        if (dto is null) return null;

        // Enrich: Vendor name
        if (dto.VendorId > 0)
        {
            var vendors = await _partyLookup.GetByIdsAsync(new[] { dto.VendorId }, ct);
            var vendor = vendors.FirstOrDefault();
            if (vendor != null) dto.VendorName = vendor.PartyName;
        }

        // Enrich: Currency name
        if (dto.CurrencyId > 0)
        {
            var currencies = await _currencyLookup.GetByIdsAsync(new[] { dto.CurrencyId }, ct);
            var currency = currencies.FirstOrDefault();
            if (currency != null) dto.CurrencyName = currency.Name;
        }

        // Enrich: Item names + UOM names in details
        if (dto.Details is { Count: > 0 })
        {
            var itemIds = dto.Details.Select(d => d.ItemId).Where(id => id > 0).Distinct().ToArray();
            if (itemIds.Length > 0)
            {
                var items = await _itemLookup.GetByIdsAsync(itemIds, ct);
                var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);
                foreach (var detail in dto.Details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemName))
                        detail.ItemName = itemName;
                }
            }
        }

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: "GetBlanketMasterByIdQuery",
            actionName: dto.Id.ToString(),
            details: $"Blanket Master details {dto.Id} was fetched.",
            module: "BlanketMaster"
        );
        await _mediator.Publish(ev, ct);

        return dto;
    }
}
