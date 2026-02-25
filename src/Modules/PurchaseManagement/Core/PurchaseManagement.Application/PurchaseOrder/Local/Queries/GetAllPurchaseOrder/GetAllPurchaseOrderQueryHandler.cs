using Contracts.Interfaces.Lookups.Budget;
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
    private readonly IPartyLookup _partyLookup;
    private readonly IBudgetGroupLookup _budgetGroupLookup;

    public GetPurchaseOrdersQueryHandler(
        IPurchaseOrderQueryRepository repo,
        IPartyLookup partyLookup,
        IBudgetGroupLookup budgetGroupLookup)
    {
        _repo = repo;
        _partyLookup = partyLookup;
        _budgetGroupLookup = budgetGroupLookup;
    }


    public async Task<PagedResult<PurchaseOrderListItemDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken ct)
    {
        // ---------- STEP 1: Fetch POs ----------
        var page = await _repo.GetAllAsync(request.PageNumber, request.PageSize,
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

        // ---------- STEP 4: Local search filter ----------
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();

            var filtered = page.Items.Where(po =>
                (po.PONumber != null && po.PONumber.ToLower().Contains(s)) ||
                (po.VendorName != null && po.VendorName.ToLower().Contains(s)) ||
                (po.StatusCode != null && po.StatusCode.ToLower().Contains(s)) ||
                (po.BudgetGroupName != null && po.BudgetGroupName.ToLower().Contains(s))
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

