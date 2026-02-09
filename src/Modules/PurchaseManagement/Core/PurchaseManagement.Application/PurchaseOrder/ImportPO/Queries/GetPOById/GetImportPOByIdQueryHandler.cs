using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetPOById;

public class GetImportPOByIdQueryHandler
    : IRequestHandler<GetImportPOByIdQuery, ImportPOFullVm?>
{
    private readonly IImportPOQueryRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly IPartyLookup _partyLookup;
    private readonly ICurrencyLookup _currencyLookup;
    private readonly IUOMLookup _uomLookup;
    private readonly IItemLookup _itemLookup;
    //private readonly IDepartmentLookup _departmentLookup;
    private readonly ICompanyLookup _companyLookup;
    private readonly IUnitLookup _unitLookup;

    public GetImportPOByIdQueryHandler(
        IImportPOQueryRepository repo,
        IIPAddressService ip,
        IPartyLookup partyLookup,
        ICurrencyLookup currencyLookup,
        IUOMLookup uomLookup,
        IItemLookup itemLookup,
      //  IDepartmentLookup departmentLookup,
        ICompanyLookup companyLookup,
        IUnitLookup unitLookup)
    {
        _repo = repo;
        _ip = ip;
        _partyLookup = partyLookup;
        _currencyLookup = currencyLookup;
        _uomLookup = uomLookup;
        _itemLookup = itemLookup;
      //  _departmentLookup = departmentLookup;
        _companyLookup = companyLookup;
        _unitLookup = unitLookup;
    }

    public async Task<ImportPOFullVm?> Handle(GetImportPOByIdQuery request, CancellationToken ct)
    {
        var vm = await _repo.GetByIdAsync(request.Id, ct);
        if (vm is null) return null;

        var header = vm.PO;

        if (header.VendorId > 0)
        {
            try
            {
                var party = await _partyLookup.GetByIdAsync(header.VendorId, ct);
                if (party != null)
                    header.VendorName = party.PartyName;
            }
            catch
            {
                // swallow vendor enrichment failures
            }
        }

        if (header.CurrencyId > 0)
        {
            try
            {
                var currencies = await _currencyLookup.GetByIdsAsync(new[] { header.CurrencyId }, ct);
                var currency = currencies.FirstOrDefault();
                if (currency != null)
                    header.CurrencyName = !string.IsNullOrWhiteSpace(currency.Code) ? currency.Code : currency.Name;
            }
            catch
            {
                // swallow currency enrichment failures
            }
        }

        if (vm.ImportDocumentList?.Count > 0)
        {
            try
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var units = await _unitLookup.GetAllUnitAsync();

                var companyMap = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName ?? string.Empty);
                var unitMap = units.ToDictionary(u => u.UnitId, u => u.UnitName ?? string.Empty);

                companyMap.TryGetValue(_ip.GetCompanyId(), out var companyName);
                unitMap.TryGetValue(_ip.GetUnitId(), out var unitName);

                foreach (var doc in vm.ImportDocumentList)
                {
                    if (string.IsNullOrWhiteSpace(doc.FileName))
                        continue;

                    var segments = new List<string>();
                    if (!string.IsNullOrWhiteSpace(doc.BasePath))
                        segments.Add(doc.BasePath.Trim('/'));
                    if (!string.IsNullOrWhiteSpace(doc.ImageFolder))
                        segments.Add(doc.ImageFolder.Trim('/'));
                    if (!string.IsNullOrWhiteSpace(companyName))
                        segments.Add(companyName.Trim('/'));
                    if (!string.IsNullOrWhiteSpace(unitName))
                        segments.Add(unitName.Trim('/'));

                    var prefix = segments.Count > 0 ? string.Join("/", segments) : null;
                    doc.UploadedPath = string.IsNullOrWhiteSpace(prefix)
                        ? null
                        : $"{prefix}/{doc.FileName}";
                }
            }
            catch
            {
                // swallow document path enrichment failures
            }
        }

        var allDetails = vm.ImportHeaders
            .SelectMany(h => h.Details ?? new List<ImportPODetailReadDto>())
            .ToList();

        if (allDetails.Count > 0)
        {
            var itemIds = allDetails.Select(d => d.ItemId).Where(id => id > 0).Distinct().ToList();
            var uomIds = allDetails.Select(d => d.UomId).Where(id => id > 0).Distinct().ToList();
          
            var itemsTask = itemIds.Count > 0
                ? _itemLookup.GetByIdsAsync(itemIds, ct)
                : Task.FromResult<IReadOnlyList<ItemLookupDto>>(new List<ItemLookupDto>());

            var uomsTask = uomIds.Count > 0
                ? _uomLookup.GetByIdsAsync(uomIds, ct)
                : Task.FromResult<IReadOnlyList<UOMLookupDto>>(new List<UOMLookupDto>());

            // var deptsTask = detailDeptIds.Count > 0
            //     ? _departmentLookup.GetByIdsAsync(detailDeptIds, ct)
            //     : Task.FromResult<IReadOnlyList<DepartmentLookupDto>>(new List<DepartmentLookupDto>());

            await Task.WhenAll(itemsTask, uomsTask);

            var items = await itemsTask;
            var uoms = await uomsTask;
            //var depts = await deptsTask;

            var itemMap = items.ToDictionary(i => i.Id, i => !string.IsNullOrWhiteSpace(i.ItemName) ? i.ItemName : i.ItemCode);
            var uomMap = uoms.ToDictionary(u => u.Id, u => !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName : u.Code);
            //var deptMap = depts.ToDictionary(d => d.DepartmentId, d => d.DepartmentName ?? string.Empty);

            foreach (var detail in allDetails)
            {
                if (detail.ItemId > 0 && itemMap.TryGetValue(detail.ItemId, out var itemName))
                    detail.ItemName = itemName;

                if (detail.UomId > 0 && uomMap.TryGetValue(detail.UomId, out var uomName))
                    detail.UomName = uomName;

                // if (detail.DepartmentId is > 0 && deptMap.TryGetValue(detail.DepartmentId.Value, out var deptName))
                //     detail.DepartmentName = deptName;
            }
        }

        return vm;
    }
}
