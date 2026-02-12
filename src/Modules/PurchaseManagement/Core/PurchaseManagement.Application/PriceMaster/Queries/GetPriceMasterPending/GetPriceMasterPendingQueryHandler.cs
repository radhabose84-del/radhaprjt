using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using Contracts.Dtos.Lookups.Users;

namespace PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending
{
    public sealed class GetPriceMasterPendingQueryHandler
        : IRequestHandler<GetPriceMasterPendingQuery, (List<PriceMasterPendingGroupDto> Items, int TotalCount)>
    {
        private readonly IPriceMasterQueryRepository _repo;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup  _workflowLookup;
        private readonly IUserLookup _usersAllLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetPriceMasterPendingQueryHandler(
            IPriceMasterQueryRepository repo,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            ICurrencyLookup currencyLookup,
            IMediator mediator,
            IWorkflowLookup workflowLookup,
            IUserLookup usersAllLookup,
            IIPAddressService ipAddressService)
        {
            _repo = repo;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _currencyLookup = currencyLookup;
            _mediator = mediator;
            _workflowLookup = workflowLookup;
            _usersAllLookup = usersAllLookup;
            _ipAddressService = ipAddressService;  
        }

        public async Task<(List<PriceMasterPendingGroupDto> Items, int TotalCount)> Handle(
            GetPriceMasterPendingQuery request, CancellationToken ct)
        {
            var (rows, _) = await _repo.GetPriceMasterPendingAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            if (rows.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (rows, 0);
            }
            
            var currentUserId = _ipAddressService.GetUserId();

             var moduleIds = rows.Select(r => r.Id).Distinct().ToList();
            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.PriceMaster.ToString(), moduleIds);

            var allowedIds = wfApprovers
                .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
                            && int.TryParse(a.ApproverValue, out var parsed)
                            && parsed == currentUserId)
                .Select(a => a.ModuleTransactionId)
                .ToHashSet();

            rows = rows.Where(r => allowedIds.Contains(r.Id)).ToList();
            if (rows.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (rows, 0);
            }

            var wfByModuleId = wfApprovers
                .GroupBy(a => a.ModuleTransactionId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var first = g.First();
                        int? approverId = null;
                        if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
                            int.TryParse(first.ApproverValue, out var a))
                            approverId = a;

                        return new
                        {
                            ApproverId = approverId,
                            ApprovalRequestId = first.ApprovalRequestId,
                            IsEdit = first.IsEdit
                        };
                    });

            // User names
            var users = await _usersAllLookup.GetAllUserAsync();
            var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

            // --- Collect IDs for enrichment ---
            var itemIds = rows.Select(r => r.ItemId).Where(id => id > 0).Distinct().ToList();
            var uomIds  = rows.Select(r => r.UomId).Where(id => id > 0).Distinct().ToList();
            // currencyIds are at LINE level
            var currencyIds = rows.SelectMany(r => r.Lines)
                                  .Select(l => l.CurrencyId)
                                  .Where(id => id > 0)
                                  .Distinct()
                                  .ToList();

            // --- Parallel gRPC calls ---
            var itemsTask      = _itemLookup.GetByIdsAsync(itemIds, ct);       // List<ItemDto>
            var uomTask = _uomLookup.GetByIdsAsync(uomIds, ct); // Task<IReadOnlyList<UOMLookupDto>>
            var currenciesTask = currencyIds.Count > 0
                ? _currencyLookup.GetByIdsAsync(currencyIds, ct)                     // IReadOnlyList<CurrencyLookupDto>
                : Task.FromResult<IReadOnlyList<CurrencyLookupDto>>(Array.Empty<CurrencyLookupDto>());

            await Task.WhenAll(itemsTask, uomTask, currenciesTask);

            // --- Lookup maps ---
            var itemNameMap = (await itemsTask)
                .GroupBy(x => x.Id)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var it = g.First();
                        // Prefer ItemName, fall back to ItemCode
                        return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName! : (it.ItemCode ?? string.Empty);
                    });

            var uomMap = (await uomTask)
                .GroupBy(u => u.Id)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var u = g.First();
                        // Prefer UOMName, fall back to Code or Id
                        return !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName! : (u.Code ?? u.Id.ToString());
                    });

            var currencyMap = (await currenciesTask)
                .GroupBy(c => c.CurrencyId)
                .ToDictionary(
                    g => g.Key,
                    g => {
                        var c = g.First();
                        return !string.IsNullOrWhiteSpace(c.Code) ? c.Code! : (c.Code ?? string.Empty); // ← FIXED
                    });

            // enrich rows
            foreach (var r in rows)
            {
                 r.Lines ??= new List<PriceMasterPendingDto>();

                if (itemNameMap.TryGetValue(r.ItemId, out var itemName))
                    r.ItemName = itemName;

                if (uomMap.TryGetValue(r.UomId, out var uomName))
                    r.UOM = uomName;

                foreach (var line in r.Lines)
                    if (currencyMap.TryGetValue(line.CurrencyId, out var cname))
                        line.CurrencyName = cname;

                // workflow (leave as-is; if your module key for PriceMaster differs from RFQ, swap it at source)
                if (wfByModuleId.TryGetValue(r.Id, out var wf))
                {
                    if (wf.ApproverId.HasValue)
                    {
                        r.ApproverId = wf.ApproverId.Value;
                        if (userLookup.TryGetValue(r.ApproverId, out var approverName))
                            r.ApproverName = approverName;
                    }
                    r.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                    r.IsEdit= wf.IsEdit;
                }
            }

            await PublishAudit(rows.Count, request, ct);
            return (rows, rows.Count);
        }

        private Task PublishAudit(int count, GetPriceMasterPendingQuery q, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll-Pending",
                    actionCode: string.Empty,
                    actionName: "PriceMasterPending",
                    details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
                    module: "PriceMaster"), ct);
    }
}
