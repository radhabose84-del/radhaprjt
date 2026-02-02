// using System.Linq; // 👈 needed
// using Contracts.Interfaces.External.IInvetoryManagement;   // IItemGrpcClient, IUOMGrpcClient, IHSNGrpcClient
// using Contracts.Interfaces.External.IUser;                  // IUsersAllGrpcClient
// using Contracts.Interfaces.External.IWorkflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending
// {
//     public sealed class GetQuoteComparisonPendingQueryHandler
//         : IRequestHandler<GetQuoteComparisonPendingQuery, (List<QuoteComparisonPendingGroupDto> Items, int TotalCount)>
//     {
//         private readonly IQuotationCompareQueryRepository _repo;
//         private readonly IItemGrpcClient _itemGrpc;
//         private readonly IUOMGrpcClient _uomGrpc;
//         private readonly IHSNGrpcClient _hsnGrpc;
//         private readonly IMediator _mediator;
//         private readonly IWorkflowGrpcClient _workflowGrpcClient;
//         private readonly IUsersAllGrpcClient _usersAllGrpcClient;
//         private readonly IIPAddressService _ipAddressService;

//         public GetQuoteComparisonPendingQueryHandler(
//             IQuotationCompareQueryRepository repo,
//             IItemGrpcClient itemGrpc,
//             IUOMGrpcClient uomGrpc,
//             IHSNGrpcClient hsnGrpc,
//             IMediator mediator,
//             IWorkflowGrpcClient workflowGrpcClient,
//             IUsersAllGrpcClient usersAllGrpcClient,
//             IIPAddressService ipAddressService
//         )
//         {
//             _repo = repo;
//             _itemGrpc = itemGrpc;
//             _uomGrpc = uomGrpc;
//             _hsnGrpc = hsnGrpc;
//             _mediator = mediator;
//             _workflowGrpcClient = workflowGrpcClient;
//             _usersAllGrpcClient = usersAllGrpcClient;
//             _ipAddressService = ipAddressService;
//         }

//         public async Task<(List<QuoteComparisonPendingGroupDto> Items, int TotalCount)> Handle(
//             GetQuoteComparisonPendingQuery request, CancellationToken ct)
//         {
//             // If your repo supports ct, pass it through.
//             var (rows, total) = await _repo.GetQuoteComparisonPendingAsync(
//                 request.PageNumber, request.PageSize, request.SearchTerm /* , ct */);

//             if (rows.Count == 0)
//             {
//                 await PublishAudit(0, request, ct);
//                 return (rows, total);
//             }
//             var currentUserId = _ipAddressService.GetUserId();

//             // --- Workflow approvers by ComparisonId ---
//                 var comparisonIds = rows
//                     .Select(r => r.Id)
//                     .Where(id => id > 0)
//                     .Distinct()
//                     .ToList();

//             var wfApprovers = await _workflowGrpcClient
//                 .GetApproverListAsync(MiscEnumEntity.QuotationComparison.ToString(), comparisonIds);

//             // ---- Allowed comparison IDs where current user is an approver
//             var allowedComparisonIds = wfApprovers
//                 .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
//                             && int.TryParse(a.ApproverValue, out var parsed)
//                             && parsed == currentUserId)
//                 .Select(a => a.ModuleTransactionId)
//                 .ToHashSet();

//             rows = rows.Where(r => allowedComparisonIds.Contains(r.Id)).ToList();
//             if (rows.Count == 0)
//             {
//                 await PublishAudit(0, request, ct);
//                 return (rows, 0); // total reflects filtered results
//             }


//             var wfByModuleId = wfApprovers
//                 .GroupBy(a => a.ModuleTransactionId)
//                 .ToDictionary(
//                     g => g.Key,
//                     g =>
//                     {
//                         var first = g.FirstOrDefault(x => int.TryParse(x.ApproverValue, out _)) ?? g.First();
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

//             // --- Gather IDs from LINES (defend against null Lines) ---
//             foreach (var g in rows)
//                 g.Lines ??= new List<QuoteComparisonPendingDto>();

//             var allLines = rows.SelectMany(g => g.Lines).ToList();

//             var itemIds = allLines.Select(l => l.ItemId).Where(id => id > 0).Distinct().ToList();
//             var uomIds  = allLines.Select(l => l.UomId).Where(id => id > 0).Distinct().ToList();
//             var hsnIds  = allLines.Select(l => l.HsnId).Where(id => id > 0).Distinct().ToList();

//             // --- Parallel gRPC (works even if some lists are empty) ---
//             var itemsTask = _itemGrpc.GetItemsByIdsAsync(itemIds, ct);          // List<ItemDto>
//             var uomTasks  = uomIds.Select(id => _uomGrpc.GetByIdAsync(id, ct)).ToArray(); // Task<UOMDto?>[]
//             var hsnTasks  = hsnIds.Select(id => _hsnGrpc.GetByIdAsync(id)).ToArray();     // Task<HSNDto?>[]

//             await Task.WhenAll(itemsTask, Task.WhenAll(uomTasks), Task.WhenAll(hsnTasks));

//             // --- Build lookup maps with safe fallbacks ---
//             var itemNameMap = (await itemsTask)
//                 .GroupBy(x => x.Id)
//                 .ToDictionary(
//                     g => g.Key,
//                     g =>
//                     {
//                         var it = g.First();
//                         return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName! : (it.ItemCode ?? string.Empty);
//                     });

//             var uomMap = uomTasks.Where(t => t.Result != null)
//                                  .Select(t => t.Result!)
//                                  .GroupBy(u => u.Id)
//                                  .ToDictionary(
//                                      g => g.Key,
//                                      g =>
//                                      {
//                                          var u = g.First();
//                                          return !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName! : (u.Code ?? u.Id.ToString());
//                                      });

//             var hsnMap = hsnTasks.Where(t => t.Result != null)
//                                  .Select(t => t.Result!)
//                                  .GroupBy(h => h.Id)
//                                  .ToDictionary(
//                                      g => g.Key,
//                                      g =>
//                                      {
//                                          var h = g.First();
//                                          return h.HSNCode ?? string.Empty;
//                                      });

//             // --- Enrich: per-line (names) and per-group (workflow) ---
//             foreach (var g in rows)
//             {
//                 foreach (var line in g.Lines)
//                 {
//                     if (itemNameMap.TryGetValue(line.ItemId, out var itemName))
//                         line.ItemName = itemName;

//                     if (uomMap.TryGetValue(line.UomId, out var uomName))
//                         line.UOM = uomName;

//                     if (hsnMap.TryGetValue(line.HsnId, out var hsnCode))
//                         line.HSNCode = hsnCode;
//                 }

//                 if (wfByModuleId.TryGetValue(g.Id, out var wf))
//                 {
//                     if (wf.ApproverId.HasValue)
//                     {
//                         g.ApproverId = wf.ApproverId.Value;
//                         if (userLookup.TryGetValue(g.ApproverId.Value, out var approverName))
//                             g.ApproverName = approverName;
//                     }
//                     g.ApprovalRequestHeaderId = wf.ApprovalRequestId;
//                     g.IsEdit = wf.IsEdit;
//                 }
//             }

//             await PublishAudit(rows.Count, request, ct);
//             return (rows, total);
//         }

//         private Task PublishAudit(int count, GetQuoteComparisonPendingQuery q, CancellationToken ct)
//             => _mediator.Publish(new AuditLogsDomainEvent(
//                 actionDetail: "GetAll-Pending",
//                 actionCode: string.Empty,
//                 actionName: "QuotationComparison",
//                 details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
//                 module: "QuotationCompare"), ct);
//     }
// }
