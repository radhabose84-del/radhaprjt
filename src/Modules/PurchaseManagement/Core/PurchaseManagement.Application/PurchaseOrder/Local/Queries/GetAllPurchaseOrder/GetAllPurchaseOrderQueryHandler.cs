using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetAllPurchaseOrder;

public class GetPurchaseOrdersQueryHandler
    : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderListItemDto>>
{
    private readonly IPurchaseOrderQueryRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly IPartyLookup _partyLookup;
    private readonly IBudgetGroupLookup _budgetGroupLookup;
    private readonly IInventoryCategoryLookup _inventoryCategoryLookup;

    public GetPurchaseOrdersQueryHandler(
        IPurchaseOrderQueryRepository repo,
        IIPAddressService ip,
        IPartyLookup partyLookup,
        IBudgetGroupLookup budgetGroupLookup,
        IInventoryCategoryLookup inventoryCategoryLookup)
    {
        _repo = repo;
        _ip = ip;
        _partyLookup = partyLookup;
        _budgetGroupLookup = budgetGroupLookup;
        _inventoryCategoryLookup = inventoryCategoryLookup;
    }


    public async Task<PagedResult<PurchaseOrderListItemDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken ct)
    {
        // Supplier-scoped view when the JWT carries a PartyId (supplier portal login).
        // Buyer / internal users have no PartyId → fall back to the buyer-scoped list (filtered by UnitId in repo).
        var partyId = _ip.GetPartyId();

        // ---------- STEP 1: Fetch POs ----------
        var page = (partyId.HasValue && partyId.Value > 0)
            ? await _repo.GetMyPurchaseOrdersAsync(partyId.Value, request.PageNumber, request.PageSize,
                request.SearchTerm, request.PoMethodId, request.StatusId, request.BudgetGroupId, ct)
            : await _repo.GetAllAsync(request.PageNumber, request.PageSize,
                request.SearchTerm, request.PoMethodId, request.StatusId, request.BudgetGroupId, ct);

        if (page.Items.Count == 0)
            return page;

        // ---------- STEP 2: Vendor Enrichment ----------
        var vendorIds = page.Items.Select(x => x.VendorId)
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        Dictionary<int, string> vendorMap = new();
        if (vendorIds.Count > 0)
        {
            try
            {
                var vendors = await _partyLookup.GetByIdsAsync(vendorIds, ct);
                vendorMap = vendors.ToDictionary(x => x.Id, x => x.PartyName ?? string.Empty);
            }
            catch
            {
                // swallow enrichment failures
            }
        }

        // Apply VendorName
        foreach (var item in page.Items)
        {
            if (vendorMap.TryGetValue(item.VendorId, out var vname))
                item.VendorName = vname;
        }

        // ---------- STEP 3: BudgetGroup Enrichment ----------
        var bgIds = page.Items
            .Select(x => x.BudgetGroupId)
            .Where(id => id.HasValue && id.Value > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<int, string> budgetGroupMap = new();
        if (bgIds.Count > 0)
        {
            try
            {
                var budgetGroups = await _budgetGroupLookup.GetByIdsAsync(bgIds, ct);
                budgetGroupMap = budgetGroups.ToDictionary(x => x.Id, x => x.Name ?? string.Empty);
            }
            catch
            {
                // swallow exception or log
            }
        }

        // Apply BudgetGroupName
        foreach (var item in page.Items)
        {
            if (item.BudgetGroupId.HasValue &&
                item.BudgetGroupId > 0 &&
                budgetGroupMap.TryGetValue(item.BudgetGroupId.Value, out var bgName))
                item.BudgetGroupName = bgName;
        }

        // ---------- STEP 3b: ItemCategory Enrichment ----------
        var catIds = page.Items
            .Select(x => x.ItemCategoryId)
            .Where(id => id.HasValue && id.Value > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<int, string> categoryMap = new();
        if (catIds.Count > 0)
        {
            try
            {
                var categories = await _inventoryCategoryLookup.GetCategoryByIdsAsync(catIds, ct);
                categoryMap = categories.ToDictionary(x => x.Id, x => x.ItemCategoryName ?? string.Empty);
            }
            catch
            {
                // swallow enrichment failures
            }
        }

        // Apply ItemCategoryName
        foreach (var item in page.Items)
        {
            if (item.ItemCategoryId.HasValue &&
                item.ItemCategoryId > 0 &&
                categoryMap.TryGetValue(item.ItemCategoryId.Value, out var catName))
                item.ItemCategoryName = catName;
        }

        // ---------- STEP 4: Local search filter ----------
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();

            var filtered = page.Items.Where(po =>
                (po.PONumber != null && po.PONumber.ToLower().Contains(s)) ||
                (po.VendorName != null && po.VendorName.ToLower().Contains(s)) ||
                (po.StatusCode != null && po.StatusCode.ToLower().Contains(s)) ||
                (po.BudgetGroupName != null && po.BudgetGroupName.ToLower().Contains(s)) ||
                (po.ItemCategoryName != null && po.ItemCategoryName.ToLower().Contains(s))
            ).ToList();

            return new PagedResult<PurchaseOrderListItemDto>
            {
                Page = request.PageNumber,
                PageSize = request.PageSize,
                Total = page.Total,
                Items = filtered
            };
        }

        // ---------- STEP 5: Return enriched result ----------
        return new PagedResult<PurchaseOrderListItemDto>
        {
            Page = request.PageNumber,
            PageSize = request.PageSize,
            Total = page.Total,
            Items = page.Items
        };
    }

}

