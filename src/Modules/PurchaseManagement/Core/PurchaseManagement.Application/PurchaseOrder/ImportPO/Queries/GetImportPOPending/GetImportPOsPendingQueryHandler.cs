using Contracts.Dtos.Party;
using Contracts.Interfaces.External.IInvetoryManagement;
using Contracts.Interfaces.External.IParty;
using Contracts.Interfaces.External.IUser;
using Contracts.Interfaces.External.IWorkflow;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending
{
    public sealed class GetImportPOsPendingQueryHandler
        : IRequestHandler<GetImportPOsPendingQuery, (List<GetPOImportPendingGroupDto> Items, int TotalCount)>
    {
        private readonly IImportPOQueryRepository _importPOQueryRepository;
        // private readonly IItemGrpcClient _itemGrpc;
        private readonly IMediator _mediator;
        // private readonly IWorkflowGrpcClient _workflowGrpcClient;
        // private readonly IUsersAllGrpcClient _usersAllGrpcClient;
        // private readonly IPartyGrpcClient _partyGrpc;
        private readonly IIPAddressService _ipAddressService;
        // private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;

        public GetImportPOsPendingQueryHandler(
            IImportPOQueryRepository importPOQueryRepository,
            // IItemGrpcClient itemGrpc,
            IMediator mediator,
            // IWorkflowGrpcClient workflowGrpcClient,
            // IUsersAllGrpcClient usersAllGrpcClient,
            // IPartyGrpcClient partyGrpc,
            IIPAddressService ipAddressService
            //, IDepartmentAllGrpcClient departmentAllGrpcClient
            )
        {
            _importPOQueryRepository = importPOQueryRepository;
            // _itemGrpc = itemGrpc;
            _mediator = mediator;
            // _workflowGrpcClient = workflowGrpcClient;
            // _usersAllGrpcClient = usersAllGrpcClient;
            // _partyGrpc = partyGrpc;
            _ipAddressService = ipAddressService;
            // _departmentAllGrpcClient = departmentAllGrpcClient;
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

            // ---------------- Filter by current user's approver id ----------------
            var currentUserId = _ipAddressService.GetUserId();

            var poIds = rows.Select(r => r.PurchaseOrderId)
                            .Where(id => id > 0)
                            .Distinct()
                            .ToList();

            // var wfApprovers = await _workflowGrpcClient
            //     .GetApproverListAsync(MiscEnumEntity.POImport.ToString(), poIds);

            // var allowedPoIds = wfApprovers
            //     .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
            //                 && int.TryParse(a.ApproverValue, out var parsed)
            //                 && parsed == currentUserId)
            //     .Select(a => a.ModuleTransactionId)
            //     .ToHashSet();

            // rows = rows.Where(r => allowedPoIds.Contains(r.PurchaseOrderId)).ToList();

            // if (rows.Count == 0)
            // {
            //     await PublishAudit(0, request, ct);
            //     return (rows, 0);
            // }

            // // Build workflow map only for kept POs
            // var wfByModuleId = wfApprovers
            //     .Where(a => allowedPoIds.Contains(a.ModuleTransactionId))
            //     .GroupBy(a => a.ModuleTransactionId)
            //     .ToDictionary(
            //         g => g.Key,
            //         g =>
            //         {
            //             var first = g.First();
            //             int? approverId = null;
            //             if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
            //                 int.TryParse(first.ApproverValue, out var parsed))
            //             {
            //                 approverId = parsed;
            //             }
            //             return new
            //             {
            //                 ApproverId = approverId,
            //                 ApprovalRequestId = first.ApprovalRequestId
            //             };
            //         });

            // // Users → names
            // var users = await _usersAllGrpcClient.GetUserAllAsync();
            // var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

            // // Ensure Lines are non-null
            // foreach (var g in rows)
            //     g.Lines ??= new List<GetPOImportPendingDto>();

            // // Collect IDs for further enrichment
            // var allLines = rows.SelectMany(g => g.Lines).ToList();
            // var itemIds = allLines.Select(l => l.ItemId).Where(id => id > 0).Distinct().ToList();
            // var vendorIds = rows.Select(r => r.VendorId).Where(v => v > 0).Distinct().ToList();
            // var deptIds = allLines.Where(l => l.DepartmentId.HasValue).Select(l => l.DepartmentId!.Value).Distinct().ToList();

            // // ---------------- Parallel enrichment calls ----------------
            // var itemsTask = _itemGrpc.GetItemsByIdsAsync(itemIds, ct); // List<ItemDto>
            // var departmentsTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
            

            // // Wait for parallel tasks to complete
            // await Task.WhenAll(itemsTask, departmentsTask);

            //   var vendorMap = new Dictionary<int, (string Name, string? Email, string? Mobile)>();
            // if (vendorIds.Count > 0)
            // {
            //     var vendorTasks = vendorIds.Select(id => _partyGrpc.GetPartyByIdAsync(id)).ToArray();
            //     await Task.WhenAll(vendorTasks);

            //     foreach (var t in vendorTasks)
            //     {
            //         if (t.Status != TaskStatus.RanToCompletion || t.Result is null) continue;
            //         var p = t.Result; // PartyDetailsDto
            //         var (email, mobile) = ResolveVendorContact(p);
            //         vendorMap[p.Id] = (p.PartyName ?? string.Empty, email, mobile);
            //     }
            // }

            // var itemNameMap = (await itemsTask).GroupBy(x => x.Id)
            //     .ToDictionary(g => g.Key, g => g.First().ItemName ?? g.First().ItemCode);

            // var departmentLookup = (departmentsTask.Result as IEnumerable<dynamic> ?? Enumerable.Empty<dynamic>())
            //     .ToDictionary(d => (int)d.DepartmentId, d => (string?)d.DepartmentName ?? string.Empty);

            // // ---------------- Enrich results ----------------
            // foreach (var g in rows)
            // {
            //     foreach (var line in g.Lines)
            //     {
            //         if (itemNameMap.TryGetValue(line.ItemId, out var itemName))
            //             line.ItemName = itemName;

            //         if (line.DepartmentId.HasValue && departmentLookup.TryGetValue(line.DepartmentId.Value, out var deptName))
            //             line.DepartmentName = deptName;
            //     }

            //     if (wfByModuleId.TryGetValue(g.PurchaseOrderId, out var wf))
            //     {
            //         if (wf.ApproverId.HasValue)
            //         {
            //             g.ApproverId = wf.ApproverId.Value; // == currentUserId by construction
            //             if (userLookup.TryGetValue(g.ApproverId.Value, out var approverName))
            //                 g.ApproverName = approverName;
            //         }
            //         g.ApprovalRequestHeaderId = wf.ApprovalRequestId;
            //     }

            //     if (g.VendorId > 0 && vendorMap.TryGetValue(g.VendorId, out var v))
            //     {
            //         g.VendorName = v.Name;
            //         g.VendorEmail = v.Email;
            //         g.VendorMobile = v.Mobile;
            //     }
            // }

            await PublishAudit(rows.Count, request, ct);
            return (rows, rows.Count);
        }
 private static (string? Email, string? Mobile) ResolveVendorContact(PartyDetailsDto party)
        {
            if (party?.Contacts == null || party.Contacts.Count == 0)
                return (null, null);

            var primary = party.Contacts.FirstOrDefault(c =>
                string.Equals(c.ContactType?.Trim(), "Primary", StringComparison.OrdinalIgnoreCase) &&
                (!string.IsNullOrWhiteSpace(c.Email) || !string.IsNullOrWhiteSpace(c.Mobile)));

            return (primary?.Email, primary?.Mobile);
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
