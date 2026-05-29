using AutoMapper;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BlanketMaster.Queries.GetAll;

public sealed class GetAllBlanketMasterQueryHandler
    : IRequestHandler<GetAllBlanketMasterQuery, PagedResult<BlanketHeaderDto>>
{
    private readonly IBlanketMasterQueryRepository _repo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IPartyLookup _partyLookup;
    private readonly ICurrencyLookup _currencyLookup;

    public GetAllBlanketMasterQueryHandler(
        IBlanketMasterQueryRepository repo,
        IMapper mapper,
        IMediator mediator,
        IPartyLookup partyLookup,
        ICurrencyLookup currencyLookup)
    {
        _repo = repo;
        _mapper = mapper;
        _mediator = mediator;
        _partyLookup = partyLookup;
        _currencyLookup = currencyLookup;
    }

    public async Task<PagedResult<BlanketHeaderDto>> Handle(
        GetAllBlanketMasterQuery request, CancellationToken ct)
    {
        var (rows, total) = await _repo.GetAllAsync(
            request.PageNumber, request.PageSize, request.Search, ct);

        var items = rows.ToList();

        // Enrich: Vendor names via cross-module lookup
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

            // Enrich: Currency names
            var currencyIds = items.Select(x => x.CurrencyId).Where(x => x > 0).Distinct().ToArray();
            if (currencyIds.Length > 0)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(currencyIds, ct);
                var currencyDict = currencies.ToDictionary(c => c.CurrencyId, c => c.Name);
                foreach (var item in items)
                {
                    if (currencyDict.TryGetValue(item.CurrencyId, out var cname))
                        item.CurrencyName = cname;
                }
            }
        }

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetAllBlanketMasterQuery",
            actionCode: "Get",
            actionName: items.Count.ToString(),
            details: "Blanket Master list fetched.",
            module: "BlanketMaster"
        );
        await _mediator.Publish(ev, ct);

        return new PagedResult<BlanketHeaderDto>
        {
            Items = items,
            Total = total,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
