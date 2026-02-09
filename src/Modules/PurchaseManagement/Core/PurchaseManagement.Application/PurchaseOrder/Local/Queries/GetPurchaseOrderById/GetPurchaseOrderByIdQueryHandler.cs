using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQueryHandler
    : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDetailDto?>
{
    private readonly IPurchaseOrderQueryRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly IPartyLookup _partyLookup;
    private readonly ICurrencyLookup _currencyLookup;
    private readonly IUOMLookup _uomLookup;
    private readonly IItemLookup _itemLookup;
    private readonly IDepartmentLookup _departmentLookup;
    private readonly ICompanyLookup _companyLookup;
    private readonly IUnitLookup _unitLookup;

    public GetPurchaseOrderByIdQueryHandler(
        IPurchaseOrderQueryRepository repo,
        IIPAddressService ip,
        IPartyLookup partyLookup,
        ICurrencyLookup currencyLookup,
        IUOMLookup uomLookup,
        IItemLookup itemLookup,
        IDepartmentLookup departmentLookup,
        ICompanyLookup companyLookup,
        IUnitLookup unitLookup)
    {
        _repo = repo;
        _ip = ip;
        _partyLookup = partyLookup;
        _currencyLookup = currencyLookup;
        _uomLookup = uomLookup;
        _itemLookup = itemLookup;
        _departmentLookup = departmentLookup;
        _companyLookup = companyLookup;
        _unitLookup = unitLookup;
    }

    public async Task<PurchaseOrderDetailDto?> Handle(GetPurchaseOrderByIdQuery r, CancellationToken ct)
    {
        var header = await _repo.GetByIdAsync(r.Id, ct);
        if (header is null) return null;

        // Enrich header with vendor name
        if (header.VendorId > 0)
        {
            try
            {
                var party = await _partyLookup.GetByIdAsync(header.VendorId, ct);
                if (party != null)
                    header.VendorName = party.PartyName;
            }
            catch { /* swallow vendor enrichment failures */ }
        }

        // Enrich header with currency name
        if (header.CurrencyId > 0)
        {
            try
            {
                var currencies = await _currencyLookup.GetByIdsAsync(new[] { header.CurrencyId }, ct);
                var currency = currencies.FirstOrDefault();
                if (currency != null)
                    header.CurrencyName = !string.IsNullOrWhiteSpace(currency.Code) ? currency.Code : currency.Name;
            }
            catch { /* swallow currency enrichment failures */ }
        }

        // Build document upload paths
        if (header.DocumentsList?.Count > 0)
        {
            try
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var units = await _unitLookup.GetAllUnitAsync();

                var companyLookupDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                var unitLookupDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                companyLookupDict.TryGetValue(_ip.GetCompanyId(), out var companyName);
                unitLookupDict.TryGetValue(_ip.GetUnitId(), out var unitName);

                companyName ??= string.Empty;
                unitName ??= string.Empty;

                // Update document paths
                foreach (var doc in header.DocumentsList)
                {
                    if (!string.IsNullOrWhiteSpace(doc.FileName))
                    {
                        // Format: ImagePath/POImage/CompanyName/UnitName/FileName
                        doc.UploadedPath = $"PoDocument/{companyName}/{unitName}/{doc.FileName}";
                    }
                }
            }
            catch { /* swallow document path enrichment failures */ }
        }

        // Enrich line details
        var allDetails = header.Headers?.SelectMany(h => h.Details ?? new List<Dtos.Local.PurchaseLocalDetailDto>()).ToList()
                         ?? new List<Dtos.Local.PurchaseLocalDetailDto>();

        if (allDetails.Count > 0)
        {
            // Collect IDs for batch lookup
            var itemIds = allDetails.Select(x => x.ItemId).Where(x => x > 0).Distinct().ToList();
            var uomIds = allDetails.Select(x => x.UOMId).Where(x => x > 0).Distinct().ToList();
            var deptIds = allDetails.Select(x => x.DepartmentId ?? 0).Where(x => x > 0).Distinct().ToList();

            // Fetch lookups in parallel
            var itemsTask = itemIds.Count > 0
                ? _itemLookup.GetByIdsAsync(itemIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>>(new List<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());

            var uomsTask = uomIds.Count > 0
                ? _uomLookup.GetByIdsAsync(uomIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>>(new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>());

            var deptsTask = deptIds.Count > 0
                ? _departmentLookup.GetByIdsAsync(deptIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>>(new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>());

            await Task.WhenAll(itemsTask, uomsTask, deptsTask);

            var items = await itemsTask;
            var uoms = await uomsTask;
            var depts = await deptsTask;

            // Build maps
            var itemMap = items.ToDictionary(i => i.Id, i => !string.IsNullOrWhiteSpace(i.ItemName) ? i.ItemName : i.ItemCode);
            var uomMap = uoms.ToDictionary(u => u.Id, u => !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName : u.Code);
            var deptMap = depts.ToDictionary(d => d.DepartmentId, d => d.DepartmentName ?? string.Empty);

            // Apply enrichment
            foreach (var d in allDetails)
            {
                if (d.ItemId > 0 && itemMap.TryGetValue(d.ItemId, out var itemName))
                    d.ItemName = itemName;

                if (d.UOMId > 0 && uomMap.TryGetValue(d.UOMId, out var uomName))
                    d.UOMName = uomName;

                if (d.DepartmentId is > 0 && deptMap.TryGetValue(d.DepartmentId.Value, out var deptName))
                    d.DepartmentName = deptName;
            }
        }

        return header;
    }
}
