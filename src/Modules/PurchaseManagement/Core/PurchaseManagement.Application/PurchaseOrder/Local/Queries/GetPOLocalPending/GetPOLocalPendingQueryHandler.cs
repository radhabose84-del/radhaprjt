// using Contracts.Dtos.Party;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWorkflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending
// {
//     public sealed class GetPOLocalPendingQueryHandler
//         : IRequestHandler<GetPOLocalPendingQuery, (List<GetPOLocalPendingGroupDto> Items, int TotalCount)>
//     {
//         private readonly IPurchaseOrderQueryRepository _repo;
//         private readonly IMediator _mediator;
//         private readonly IWorkflowGrpcClient _workflowGrpcClient;
//         private readonly IUsersAllGrpcClient _usersAllGrpcClient;       
//         private readonly IIPAddressService _ipAddressService; 
//         private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
//         private readonly IItemGrpcClient _itemGrpc;
//         private readonly IPartyGrpcClient _partyGrpc;


//         public GetPOLocalPendingQueryHandler(
//             IPurchaseOrderQueryRepository repo,           
//             IMediator mediator,
//             IWorkflowGrpcClient workflowGrpcClient,
//             IUsersAllGrpcClient usersAllGrpcClient,            
//             IIPAddressService ipAddressService, IDepartmentAllGrpcClient departmentAllGrpcClient,
//             IItemGrpcClient itemGrpcClient,
//             IPartyGrpcClient partyGrpc)
//         {
//             _repo = repo;          
//             _mediator = mediator;
//             _workflowGrpcClient = workflowGrpcClient;
//             _usersAllGrpcClient = usersAllGrpcClient;            
//             _ipAddressService = ipAddressService;
//             _departmentAllGrpcClient = departmentAllGrpcClient; 
//             _itemGrpc = itemGrpcClient;
//             _partyGrpc = partyGrpc;
//         }

//         public async Task<(List<GetPOLocalPendingGroupDto> Items, int TotalCount)> Handle(
//             GetPOLocalPendingQuery request, CancellationToken ct)
//         {
//             var (rows, _) = await _repo.GetPOPendingAsync(
//                 request.PageNumber ?? 1, request.PageSize ?? 10, request.SearchTerm, request.PoId,request.PoMethodId, ct);

//             if (rows.Count == 0)
//             {
//                 await PublishAudit(0, request, ct);  
//                 return (rows, 0);
//             }

//             // ---------------- Filter by current user's approver id ----------------
//             var currentUserId = _ipAddressService.GetUserId();

//             var poIds = rows.Select(r => r.Id)
//                             .Where(id => id > 0)
//                             .Distinct()
//                             .ToList();

//             var wfApprovers = await _workflowGrpcClient
//                 .GetApproverListAsync(MiscEnumEntity.POLocal.ToString(), poIds);

//             var allowedPoIds = wfApprovers
//                 .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
//                             && int.TryParse(a.ApproverValue, out var parsed)
//                             && parsed == currentUserId)
//                 .Select(a => a.ModuleTransactionId)
//                 .ToHashSet();

//             rows = rows.Where(r => allowedPoIds.Contains(r.Id)).ToList();
            

//             if (rows.Count == 0)
//             {
//                 await PublishAudit(0, request, ct);
//                 return (rows, 0);
//             }

//             // Build workflow map only for kept POs
//             var wfByModuleId = wfApprovers
//                 .Where(a => allowedPoIds.Contains(a.ModuleTransactionId))
//                 .GroupBy(a => a.ModuleTransactionId)
//                 .ToDictionary(
//                     g => g.Key,
//                     g =>
//                     {
//                         var first = g.First();
//                         int? approverId = null;
//                         if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
//                             int.TryParse(first.ApproverValue, out var parsed))
//                         {
//                             approverId = parsed;
//                         }
//                         return new
//                         {
//                             ApproverId = approverId,
//                             ApprovalRequestId = first.ApprovalRequestId,
//                             IsEdit = first.IsEdit

//                         };
//                     });

//             // Users → names
//             var users = await _usersAllGrpcClient.GetUserAllAsync();
//             var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

//             // Ensure Lines non-null
//             foreach (var g in rows)
//                 g.Lines ??= new List<GetPOLocalPendingDto>();

//             // Collect IDs (from filtered rows only)
//             var allLines = rows.SelectMany(g => g.Lines).ToList();
//             var itemIds = allLines.Select(l => l.ItemId).Where(id => id > 0).Distinct().ToList();
//             var vendorIds = rows.Select(r => r.VendorId).Where(v => v > 0).Distinct().ToList();
//             var deptIds = allLines.Where(l => l.DepartmentId.HasValue).Select(l => l.DepartmentId!.Value).Distinct().ToList();

//             // ---------------- Parallel enrichment calls ----------------
//             var itemsTask = _itemGrpc.GetItemsByIdsAsync(itemIds, ct); // List<ItemDto>
//             var departmentsTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
//             await Task.WhenAll(itemsTask, departmentsTask);
//             // Vendors via gRPC (fan-out)
//             var vendorMap = new Dictionary<int, (string Name, string? Email, string? Mobile)>();
//             if (vendorIds.Count > 0)
//             {
//                 var vendorTasks = vendorIds.Select(id => _partyGrpc.GetPartyByIdAsync(id)).ToArray();
//                 await Task.WhenAll(vendorTasks);

//                 foreach (var t in vendorTasks)
//                 {
//                     if (t.Status != TaskStatus.RanToCompletion || t.Result is null) continue;
//                     var p = t.Result; // PartyDetailsDto
//                     var (email, mobile) = ResolveVendorContact(p);
//                     vendorMap[p.Id] = (p.PartyName ?? string.Empty, email, mobile);
//                 }
//             }
            
//             // Items map
//             var itemNameMap = (await itemsTask)
//                 .GroupBy(x => x.Id)
//                 .ToDictionary(
//                     g => g.Key,
//                     g =>
//                     {
//                         var it = g.First();
//                         return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName! : (it.ItemCode ?? string.Empty);
//                     });
//             var deps = departmentsTask.Result as IEnumerable<dynamic> ?? Enumerable.Empty<dynamic>();
//             var departmentLookup = deps.ToDictionary(
//                     d => (int)d.DepartmentId,
//                     d => (string?)d.DepartmentName ?? string.Empty);

//             // ---------------- Enrich results ----------------
//             foreach (var g in rows)
//             {
//                 foreach (var line in g.Lines)
//                 {
//                     if (itemNameMap.TryGetValue(line.ItemId, out var itemName))
//                         line.ItemName = itemName;
//                     if (line.DepartmentId.HasValue &&
//                        departmentLookup.TryGetValue(line.DepartmentId.Value, out var deptName))
//                         line.DepartmentName = deptName;
//                 }

//                 if (wfByModuleId.TryGetValue(g.Id, out var wf))
//                 {
//                     if (wf.ApproverId.HasValue)
//                     {
//                         g.ApproverId = wf.ApproverId.Value; // == currentUserId by construction
//                         if (userLookup.TryGetValue(g.ApproverId.Value, out var approverName))
//                             g.ApproverName = approverName;
//                     }
//                     g.ApprovalRequestHeaderId = wf.ApprovalRequestId;
//                     g.IsEdit=wf.IsEdit;
//                 }

//                 if (g.VendorId > 0 && vendorMap.TryGetValue(g.VendorId, out var v))
//                 {
//                     g.VendorName = v.Name;
//                     g.VendorEmail = v.Email;
//                     g.VendorMobile = v.Mobile;
//                 }
//             }

//             await PublishAudit(rows.Count, request, ct);
//             return (rows, rows.Count);
//         }

//         private static (string? Email, string? Mobile) ResolveVendorContact(PartyDetailsDto party)
//         {
//             if (party?.Contacts == null || party.Contacts.Count == 0)
//                 return (null, null);

//             var primary = party.Contacts.FirstOrDefault(c =>
//                 string.Equals(c.ContactType?.Trim(), "Primary", StringComparison.OrdinalIgnoreCase) &&
//                 (!string.IsNullOrWhiteSpace(c.Email) || !string.IsNullOrWhiteSpace(c.Mobile)));

//             return (primary?.Email, primary?.Mobile);
//         }

//         private Task PublishAudit(int count, GetPOLocalPendingQuery q, CancellationToken ct)
//             => _mediator.Publish(new AuditLogsDomainEvent(
//                 actionDetail: "GetAll-Pending",
//                 actionCode: string.Empty,
//                 actionName: "QuotationComparison",
//                 details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
//                 module: "QuotationCompare"), ct);
//     }
// }
