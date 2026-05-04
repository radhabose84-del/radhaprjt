using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending
{
    public sealed class GetPOLocalPendingQueryHandler
        : IRequestHandler<GetPOLocalPendingQuery, (List<GetPOLocalPendingGroupDto> Items, int TotalCount)>
    {
        private readonly IPurchaseOrderQueryRepository _repo;
        private readonly IMediator _mediator;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IPartyLookup _partyLookup;

        public GetPOLocalPendingQueryHandler(
            IPurchaseOrderQueryRepository repo,
            IMediator mediator,
            IUserLookup userLookup,
            IIPAddressService ipAddressService,
            IDepartmentLookup departmentLookup,
            IItemLookup itemLookup,
            IPartyLookup partyLookup)
        {
            _repo = repo;
            _mediator = mediator;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
            _departmentLookup = departmentLookup;
            _itemLookup = itemLookup;
            _partyLookup = partyLookup;
        }

        public async Task<(List<GetPOLocalPendingGroupDto> Items, int TotalCount)> Handle(
            GetPOLocalPendingQuery request, CancellationToken ct)
        {
            var (rows, total) = await _repo.GetPOPendingAsync(
                request.PageNumber ?? 1, request.PageSize ?? 15, request.SearchTerm, request.PoId, request.PoMethodId, ct);

            if (rows.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (rows, 0);
            }

            // Ensure Lines non-null
            foreach (var g in rows)
                g.Lines ??= new List<GetPOLocalPendingDto>();

            // Collect IDs
            var allLines = rows.SelectMany(g => g.Lines).ToList();
            var itemIds = allLines.Select(l => l.ItemId).Where(id => id > 0).Distinct().ToList();
            var vendorIds = rows.Select(r => r.VendorId).Where(v => v > 0).Distinct().ToList();

            // Parallel enrichment calls
            var itemsTask = _itemLookup.GetByIdsAsync(itemIds, ct);
            var departmentsTask = _departmentLookup.GetAllDepartmentAsync();
            var vendorsTask = _partyLookup.GetByIdsAsync(vendorIds, ct);
            await Task.WhenAll(itemsTask, departmentsTask, vendorsTask);

            // Build vendor map
            var vendorMap = (await vendorsTask)
                .Where(p => p != null)
                .ToDictionary(
                    p => p.Id,
                    p => (Name: p.PartyName ?? string.Empty, Email: p.Email, Mobile: p.Mobile));

            // Items map
            var itemNameMap = (await itemsTask)
                .GroupBy(x => x.Id)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var it = g.First();
                        return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName : (it.ItemCode ?? string.Empty);
                    });

            // Departments map
            var departments = await departmentsTask;
            var departmentLookupDict = departments.ToDictionary(
                d => d.DepartmentId,
                d => d.DepartmentName ?? string.Empty);

            // Enrich results
            foreach (var g in rows)
            {
                foreach (var line in g.Lines)
                {
                    if (itemNameMap.TryGetValue(line.ItemId, out var itemName))
                        line.ItemName = itemName;
                    if (line.DepartmentId.HasValue &&
                       departmentLookupDict.TryGetValue(line.DepartmentId.Value, out var deptName))
                        line.DepartmentName = deptName;
                }

                if (g.VendorId > 0 && vendorMap.TryGetValue(g.VendorId, out var v))
                {
                    g.VendorName = v.Name;
                    g.VendorEmail = v.Email;
                    g.VendorMobile = v.Mobile;
                }
            }

            await PublishAudit(rows.Count, request, ct);
            return (rows, total);
        }

        private Task PublishAudit(int count, GetPOLocalPendingQuery q, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll-Pending",
                actionCode: string.Empty,
                actionName: "PurchaseOrderPending",
                details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
                module: "PurchaseOrder"), ct);
    }
}
