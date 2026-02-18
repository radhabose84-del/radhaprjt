#nullable disable
using AutoMapper;
using MediatR;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;

namespace PurchaseManagement.Application.PriceMaster.Queries.GetById
{
    public class GetPriceMasterByIdQueryHandler
        : IRequestHandler<GetPriceMasterByIdQuery, PriceMasterGetAllDto>
    {
        private readonly IPriceMasterQueryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public GetPriceMasterByIdQueryHandler(
            IPriceMasterQueryRepository repo,
            IMapper mapper,
            IMediator mediator,
        IItemLookup itemLookup,
        IUOMLookup uomLookup,
        IPartyLookup partyLookup,
        ICurrencyLookup currencyLookup)
        {
            _repo = repo;
            _mapper = mapper;
            _mediator = mediator;
        _itemLookup = itemLookup;
        _uomLookup = uomLookup;
        _partyLookup = partyLookup;
        _currencyLookup = currencyLookup;
        }

        public async Task<PriceMasterGetAllDto> Handle(GetPriceMasterByIdQuery request, CancellationToken ct)
        {
            var baseDto = await _repo.GetByIdAsync(request.Id, ct);
            if (baseDto is null) return null;

            // ---- batch IDs ----
            var itemIds = new[] { baseDto.ItemId };
            var uomIds  = baseDto.UomId > 0 ? new[] { baseDto.UomId } : Array.Empty<int>();
            var currencyIds = (baseDto.Details ?? Enumerable.Empty<PriceMasterDetailUpsertDto>())
                                .Select(d => d.CurrencyId)
                                .Where(id => id > 0)
                                .Distinct()
                                .ToList();

            // ---- kick off gRPC in parallel ----
            var itemsTask      = _itemLookup.GetByIdsAsync(itemIds, ct); // List<ItemDto>
            var uomsTask       = _uomLookup.GetAllAsync();                    // List<UOMDto>
            var partyIds = baseDto.VendorId > 0 ? new[] { baseDto.VendorId } : Array.Empty<int>();
            var partyTask      = _partyLookup.GetByIdsAsync(partyIds, ct);
            var currenciesTask = _currencyLookup.GetByIdsAsync(currencyIds, ct);    // List<CurrencyDto>

            await Task.WhenAll(itemsTask, uomsTask, partyTask, currenciesTask);

            // ---- maps ----
            var itemMap = (await itemsTask).ToDictionary(i => i.Id, i => i);
            var uomMap  = (await uomsTask).ToDictionary(u => u.Id, u => u);
            var partyList = await partyTask; // list
            var party = partyList.FirstOrDefault();
            var currencyMap = (await currenciesTask).ToDictionary(c => c.CurrencyId, c => c);

            // ---- enrich details with currency name ----
            if (baseDto.Details is not null && baseDto.Details.Count > 0)
            {
                foreach (var det in baseDto.Details)
                {
                    if (det.CurrencyId > 0 && currencyMap.TryGetValue(det.CurrencyId, out var cur))
                    {
                        det.CurrencyName = !string.IsNullOrWhiteSpace(cur.Code) ? cur.Code : cur.Code;
                    }
                }
            }

            // ---- resolve header names/codes ----
            itemMap.TryGetValue(baseDto.ItemId, out var itemDto);

            string uomName = null;
            if (baseDto.UomId > 0 && uomMap.TryGetValue(baseDto.UomId, out var uom))
                uomName = uom.UOMName ?? uom.Code ?? uom.UOMName;

            // (optional) derive header-level currency if ALL details share the same id
            // If your PriceMasterGetAllDto does NOT expose CurrencyId/Name at header, you can delete this block.
            int? headerCurrencyId = null;
            string headerCurrencyName = null;
            if (baseDto.Details is { Count: > 0 })
            {
                var distinctCurIds = baseDto.Details.Select(d => d.CurrencyId).Where(id => id > 0).Distinct().ToList();
                if (distinctCurIds.Count == 1)
                {
                    headerCurrencyId = distinctCurIds[0];
                    if (currencyMap.TryGetValue(headerCurrencyId.Value, out var cur))
                        headerCurrencyName = !string.IsNullOrWhiteSpace(cur.Code) ? cur.Code : cur.Code;
                }
            }

            // ---- build enriched dto ----
            var enriched = new PriceMasterGetAllDto
            {
                Id = baseDto.Id,
                ItemId = baseDto.ItemId,
                ItemCode = itemDto?.ItemCode ?? baseDto.ItemCode,
                ItemName = itemDto?.ItemName ?? baseDto.ItemName,
                VendorId = baseDto.VendorId,
                VendorCode = party?.PartyCode ?? baseDto.VendorCode,
                VendorName = party?.PartyName ?? baseDto.VendorName,
                ValidFrom = baseDto.ValidFrom,
                ValidTo = baseDto.ValidTo,
                StatusId = baseDto.StatusId,
                StatusName = baseDto.StatusName,
                SourceFromId = baseDto.SourceFromId,
                SourceFrom = baseDto.SourceFrom,
                SourceDetailId = baseDto.SourceDetailId,
                UomId = baseDto.UomId,
                UOM = uomName,
                Details = baseDto.Details
            };

            // If your header DTO has CurrencyId/CurrencyName and you want to set them:
            // enriched.CurrencyId   = headerCurrencyId ?? 0;
            // enriched.CurrencyName = headerCurrencyName;

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPriceMasterByIdQuery",
                actionName: enriched.Id.ToString(),
                details: $"PriceMaster {enriched.Id} fetched.",
                module: "PriceMaster"), ct);

            return enriched;
        }
    }
}
