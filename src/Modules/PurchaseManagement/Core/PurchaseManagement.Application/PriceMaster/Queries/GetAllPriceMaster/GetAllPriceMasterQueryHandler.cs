// PurchaseManagement.Application/PriceMaster/Handlers/GetAllPriceMasterQueryHandler.cs
using MediatR;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetAll;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.Common;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Dtos.Lookups.Party;               // ICurrencyGrpcClient (your currency client lives in "User" svc per your setup)
public sealed class GetAllPriceMasterQueryHandler
    : IRequestHandler<GetAllPriceMasterQuery, PagedResult<PriceMasterGetAllDto>>
{
    private readonly IPriceMasterQueryRepository _repo;
    private readonly IItemLookup _itemLookup;
    private readonly IUOMLookup _uomLookup;
    private readonly IPartyLookup _partyLookup;
    private readonly ICurrencyLookup _currencyLookup;

    public GetAllPriceMasterQueryHandler(
        IPriceMasterQueryRepository repo,
        IItemLookup itemLookup,
        IUOMLookup uomLookup,
        IPartyLookup partyLookup,
        ICurrencyLookup currencyLookup)
    {
        _repo = repo;
        _itemLookup = itemLookup;
        _uomLookup = uomLookup;
        _partyLookup = partyLookup;
        _currencyLookup = currencyLookup;
    }

    public async Task<PagedResult<PriceMasterGetAllDto>> Handle(GetAllPriceMasterQuery request, CancellationToken ct)
    {
        // Step 1: Fetch PriceMaster Headers from the Repository
        var page = await _repo.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.ItemId, request.QtyFrom, request.QtyTo,request.statusId,request.expiredList, ct);
        if (page.Items.Count == 0) return page;

        // Collect IDs for related entities
        var itemIds = page.Items.Select(x => x.ItemId).Where(x => x > 0).Distinct().ToList();
        var vendorIds = page.Items.Select(x => x.VendorId).Where(x => x > 0).Distinct().ToList();
        var uomIds = page.Items.Select(x => x.UomId).Where(x => x > 0).Distinct().ToList();
        var currencyIds = page.Items
            .SelectMany(x => x.Details ?? Enumerable.Empty<PriceMasterDetailUpsertDto>())
            .Select(d => d.CurrencyId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // Step 2: Parallel gRPC calls
        var itemsTask = _itemLookup.GetByIdsAsync(itemIds, ct);  // Fetch all Items via gRPC
        var uomsTask = _uomLookup.GetAllAsync();                     // Fetch all UOMs
        var partiesTask = _partyLookup.GetByIdsAsync(vendorIds, ct);     // Fetch all parties via gRPC
        var currenciesTask = _currencyLookup.GetByIdsAsync(currencyIds, ct); // Fetch all currencies via gRPC

        await Task.WhenAll(itemsTask, uomsTask, partiesTask, currenciesTask);

        // Maps to hold the results from gRPC
        var itemMap = (await itemsTask)
            .GroupBy(i => i.Id).ToDictionary(g => g.Key, g => g.First());
        var uomMap = (await uomsTask)
            .GroupBy(u => u.Id).ToDictionary(g => g.Key, g => g.First());
        var partyMap = (await partiesTask)
            .GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First());
        var currencyMap = (await currenciesTask)
            .GroupBy(c => c.CurrencyId).ToDictionary(g => g.Key, g => g.First());

        // Step 3: Enrich headers with external data (Items, UOM, Party, Currency)
        foreach (var dto in page.Items)
        {
            // Enrich with Item data
            if (itemMap.TryGetValue(dto.ItemId, out var itm))
            {
                dto.ItemCode = itm.ItemCode ?? dto.ItemCode;
                dto.ItemName = itm.ItemName ?? dto.ItemName;
            }

            // Enrich with Vendor data
            if (partyMap.TryGetValue(dto.VendorId, out var party))
            {
                dto.VendorCode = party.PartyCode ?? dto.VendorCode;
                dto.VendorName = party.PartyName ?? dto.VendorName;
            }

            // Enrich with UOM data
            if (dto.UomId > 0 && uomMap.TryGetValue(dto.UomId, out var uom))
            {
                dto.UOM = uom.UOMName ?? uom.Code ?? dto.UOM;
            }

            // Enrich details with CurrencyName
            if (dto.Details is { Count: > 0 })
            {
                foreach (var det in dto.Details)
                {
                    if (det.CurrencyId > 0 && currencyMap.TryGetValue(det.CurrencyId, out var cur))
                    {
                        det.CurrencyName = cur.Code ?? cur.Code ?? det.CurrencyName;
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();

            bool Contains(string? source)
                => !string.IsNullOrEmpty(source) &&
                source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

            var filteredItems = page.Items.Where(h =>
                Contains(h.ItemCode) ||
                Contains(h.ItemName) ||
                Contains(h.VendorName) ||
                Contains(h.StatusName) ||
                Contains(h.UOM)
            ).ToList();

            return new PagedResult<PriceMasterGetAllDto>
            {
                Items = filteredItems,
                Total = page.Total,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        // Step 5: Return the final result with the original items if no searchTerm filter
        return new PagedResult<PriceMasterGetAllDto>
        {
            Items = page.Items, 
            Total = page.Total, 
            Page = page.Page,   
            PageSize = page.PageSize 
        };
    }

    private async Task<List<PartyLookupDto>> GetByIdAsync(List<int> ids, CancellationToken ct)
    {
        var tasks = ids.Select(id => _partyLookup.GetByIdAsync(id)).ToArray();
        var results = await Task.WhenAll(tasks);
        return results.Where(p => p != null).ToList()!;
    }
}
