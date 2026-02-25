#nullable disable
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending
{
    public sealed class GetImportPOsPendingQueryHandler
        : IRequestHandler<GetImportPOsPendingQuery, (List<GetPOImportPendingGroupDto> Items, int TotalCount)>
    {
        private readonly IImportPOQueryRepository _importPOQueryRepository;
        private readonly IItemLookup _itemLookup;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDepartmentLookup _departmentLookup;

        public GetImportPOsPendingQueryHandler(
            IImportPOQueryRepository importPOQueryRepository,
            IItemLookup itemLookup,
            IMediator mediator,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IPartyLookup partyLookup,
            IIPAddressService ipAddressService,
            IDepartmentLookup departmentLookup)
        {
            _importPOQueryRepository = importPOQueryRepository;
            _itemLookup = itemLookup;
            _mediator = mediator;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _partyLookup = partyLookup;
            _ipAddressService = ipAddressService;
            _departmentLookup = departmentLookup;
        }

        public async Task<(List<GetPOImportPendingGroupDto> Items, int TotalCount)> Handle(
            GetImportPOsPendingQuery request, CancellationToken ct)
        {
            // Fetch pending Import POs from repository
            var (rows, _) = await _importPOQueryRepository.GetImportPOPendingAsync(
                request.PageNumber ?? 1, request.PageSize ?? 10, request.SearchTerm, request.PoId, ct);

            if (rows.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (rows, 0);
            }

            // Filter by current user's approver id
            var currentUserId = _ipAddressService.GetUserId();

            var poIds = rows.Select(r => r.PurchaseOrderId)
                            .Where(id => id > 0)
                            .Distinct()
                            .ToList();

            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.POImport.ToString(), poIds);

            var allowedPoIds = wfApprovers
                .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
                            && int.TryParse(a.ApproverValue, out var parsed)
                            && parsed == currentUserId)
                .Select(a => a.ModuleTransactionId)
                .ToHashSet();

            rows = rows.Where(r => allowedPoIds.Contains(r.PurchaseOrderId)).ToList();

            if (rows.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (rows, 0);
            }

            // Build workflow map only for kept POs
            var wfByModuleId = wfApprovers
                .Where(a => allowedPoIds.Contains(a.ModuleTransactionId))
                .GroupBy(a => a.ModuleTransactionId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var first = g.First();
                        int? approverId = null;
                        if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
                            int.TryParse(first.ApproverValue, out var parsed))
                        {
                            approverId = parsed;
                        }
                        return new
                        {
                            ApproverId = approverId,
                            ApprovalRequestId = first.ApprovalRequestId
                        };
                    });

            // Users → names
            var users = await _userLookup.GetAllUserAsync();
            var userMap = users.ToDictionary(u => u.UserId, u => u.UserName);

            // Ensure Lines are non-null
            foreach (var g in rows)
                g.Lines ??= new List<GetPOImportPendingDto>();

            // Collect IDs for further enrichment
            var allLines = rows.SelectMany(g => g.Lines).ToList();
            var itemIds = allLines.Select(l => l.ItemId).Where(id => id > 0).Distinct().ToList();
            var vendorIds = rows.Select(r => r.VendorId).Where(v => v > 0).Distinct().ToList();

            // Parallel enrichment calls
            var itemsTask = _itemLookup.GetByIdsAsync(itemIds, ct);
            var departmentsTask = _departmentLookup.GetAllDepartmentAsync();
            var vendorsTask = _partyLookup.GetByIdsAsync(vendorIds, ct);

            // Wait for parallel tasks to complete
            await Task.WhenAll(itemsTask, departmentsTask, vendorsTask);

            var itemNameMap = itemsTask.Result
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().ItemName ?? g.First().ItemCode);

            var departmentMap = departmentsTask.Result
                .ToDictionary(d => d.DepartmentId, d => d.DepartmentName ?? string.Empty);

            var vendorMap = vendorsTask.Result
                .Where(p => p != null)
                .ToDictionary(p => p.Id, p => (Name: p.PartyName ?? string.Empty, Email: p.Email, Mobile: p.Mobile));

            // Enrich results
            foreach (var g in rows)
            {
                foreach (var line in g.Lines)
                {
                    if (itemNameMap.TryGetValue(line.ItemId, out var itemName))
                        line.ItemName = itemName;

                    if (line.DepartmentId.HasValue && departmentMap.TryGetValue(line.DepartmentId.Value, out var deptName))
                        line.DepartmentName = deptName;
                }

                if (wfByModuleId.TryGetValue(g.PurchaseOrderId, out var wf))
                {
                    if (wf.ApproverId.HasValue)
                    {
                        g.ApproverId = wf.ApproverId.Value;
                        if (userMap.TryGetValue(g.ApproverId.Value, out var approverName))
                            g.ApproverName = approverName;
                    }
                    g.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                }

                if (g.VendorId > 0 && vendorMap.TryGetValue(g.VendorId, out var v))
                {
                    g.VendorName = v.Name;
                    g.VendorEmail = v.Email;
                    g.VendorMobile = v.Mobile;
                }
            }

            await PublishAudit(rows.Count, request, ct);
            return (rows, rows.Count);
        }
        // Helper method to publish audit logs
        private Task PublishAudit(int count, GetImportPOsPendingQuery request, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll-Pending",
                actionCode: string.Empty,
                actionName: "ImportPOPending",
                details: $"Fetched {count} rows. Page={request.PageNumber}, Size={request.PageSize}, Search='{request.SearchTerm ?? ""}'.",
                module: "PurchaseOrder"), ct);
    }
}
