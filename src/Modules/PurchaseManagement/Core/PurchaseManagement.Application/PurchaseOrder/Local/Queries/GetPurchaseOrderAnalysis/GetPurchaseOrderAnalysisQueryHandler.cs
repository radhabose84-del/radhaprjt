using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAnalysis;

public class GetPurchaseOrderAnalysisQueryHandler
    : IRequestHandler<GetPurchaseOrderAnalysisQuery, PagedResult<PurchaseOrderAnalysisListItemDto>>
{
    private readonly IPurchaseOrderQueryRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly IPartyLookup _partyLookup;
    private readonly IUnitLookup _unitLookup;
    private readonly IBudgetGroupLookup _budgetGroupLookup;
    private readonly IInventoryCategoryLookup _inventoryCategoryLookup;

    public GetPurchaseOrderAnalysisQueryHandler(
        IPurchaseOrderQueryRepository repo,
        IIPAddressService ip,
        IPartyLookup partyLookup,
        IUnitLookup unitLookup,
        IBudgetGroupLookup budgetGroupLookup,
        IInventoryCategoryLookup inventoryCategoryLookup)
    {
        _repo = repo;
        _ip = ip;
        _partyLookup = partyLookup;
        _unitLookup = unitLookup;
        _budgetGroupLookup = budgetGroupLookup;
        _inventoryCategoryLookup = inventoryCategoryLookup;
    }

    public async Task<PagedResult<PurchaseOrderAnalysisListItemDto>> Handle(
        GetPurchaseOrderAnalysisQuery request, CancellationToken ct)
    {
        // Supplier-scoped view when the JWT carries a PartyId (supplier portal login);
        // buyer / internal users (no PartyId) get the unit-scoped list.
        var partyId = _ip.GetPartyId();
        var vendorScope = (partyId.HasValue && partyId.Value > 0) ? partyId : null;

        var page = await _repo.GetAnalysisAsync(
            request.PageNumber, request.PageSize, request.SearchTerm,
            request.PoMethodId, request.StatusId,
            request.FromDate, request.ToDate, request.IsAmendment,
            vendorScope, ct);

        if (page.Items.Count == 0)
            return page;

        // ---------- Vendor enrichment ----------
        var vendorIds = page.Items.Select(x => x.VendorId).Where(x => x > 0).Distinct().ToList();
        if (vendorIds.Count > 0)
        {
            try
            {
                var vendors = await _partyLookup.GetByIdsAsync(vendorIds, ct);
                var vendorMap = vendors.ToDictionary(x => x.Id, x => x.PartyName ?? string.Empty);
                foreach (var item in page.Items)
                    if (vendorMap.TryGetValue(item.VendorId, out var vname))
                        item.VendorName = vname;
            }
            catch { /* swallow enrichment failures */ }
        }

        // ---------- Unit enrichment ----------
        try
        {
            var units = await _unitLookup.GetAllUnitAsync();
            var unitMap = units.ToDictionary(u => u.UnitId, u => u.UnitName ?? string.Empty);
            foreach (var item in page.Items)
                if (unitMap.TryGetValue(item.UnitId, out var uname))
                    item.UnitName = uname;
        }
        catch { /* swallow enrichment failures */ }

        // ---------- BudgetGroup enrichment ----------
        var bgIds = page.Items
            .Where(x => x.BudgetGroupId is > 0)
            .Select(x => x.BudgetGroupId!.Value)
            .Distinct()
            .ToList();
        if (bgIds.Count > 0)
        {
            try
            {
                var budgetGroups = await _budgetGroupLookup.GetByIdsAsync(bgIds, ct);
                var budgetGroupMap = budgetGroups.ToDictionary(x => x.Id, x => x.Name ?? string.Empty);
                foreach (var item in page.Items)
                    if (item.BudgetGroupId is > 0 &&
                        budgetGroupMap.TryGetValue(item.BudgetGroupId.Value, out var bgName))
                        item.BudgetGroupName = bgName;
            }
            catch { /* swallow enrichment failures */ }
        }

        // ---------- ItemCategory enrichment ----------
        var catIds = page.Items
            .Where(x => x.ItemCategoryId is > 0)
            .Select(x => x.ItemCategoryId!.Value)
            .Distinct()
            .ToList();
        if (catIds.Count > 0)
        {
            try
            {
                var categories = await _inventoryCategoryLookup.GetCategoryByIdsAsync(catIds, ct);
                var categoryMap = categories.ToDictionary(x => x.Id, x => x.ItemCategoryName ?? string.Empty);
                foreach (var item in page.Items)
                    if (item.ItemCategoryId is > 0 &&
                        categoryMap.TryGetValue(item.ItemCategoryId.Value, out var catName))
                        item.ItemCategoryName = catName;
            }
            catch { /* swallow enrichment failures */ }
        }

        return page;
    }
}
