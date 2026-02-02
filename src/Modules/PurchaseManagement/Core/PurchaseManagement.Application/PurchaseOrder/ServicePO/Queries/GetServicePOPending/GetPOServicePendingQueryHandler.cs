// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Dtos.Party;
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IUser;
// using Contracts.Interfaces.External.IWorkflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Events;
// using MediatR;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending
// {
//     public class GetPOServicePendingQueryHandler : IRequestHandler<GetPOServicePendingQuery, (List<GetServicePOPendingGroupDto> Items, int TotalCount)>
//     {
//         private readonly IServicePurchaseOrderQueryRepository _repo; // contains GetServicePOPendingAsync
//         private readonly IMediator _mediator;
//         private readonly IWorkflowGrpcClient _workflowGrpcClient;
//         private readonly IUsersAllGrpcClient _usersAllGrpcClient;
//         private readonly IPartyGrpcClient _partyGrpc;
//         private readonly IIPAddressService _ipAddressService;

//         public GetPOServicePendingQueryHandler(
//             IServicePurchaseOrderQueryRepository repo,
//             IMediator mediator,
//             IWorkflowGrpcClient workflowGrpcClient,
//             IUsersAllGrpcClient usersAllGrpcClient,
//             IPartyGrpcClient partyGrpc,
//             IIPAddressService ipAddressService)
//         {
//             _repo = repo;
//             _mediator = mediator;
//             _workflowGrpcClient = workflowGrpcClient;
//             _usersAllGrpcClient = usersAllGrpcClient;
//             _partyGrpc = partyGrpc;
//             _ipAddressService = ipAddressService;
//         }

//         public async Task<(List<GetServicePOPendingGroupDto> Items, int TotalCount)> Handle(
//             GetPOServicePendingQuery request, CancellationToken ct)
//         {
//             // 1) Fetch raw pending Service-PO rows (paged)
//             var (rows, _) = await _repo.GetServicePOPendingAsync(
//                 request.PageNumber ?? 1,
//                 request.PageSize   ?? 15,
//                 request.SearchTerm,
//                 request.PoId,
//                 ct);

//             if (rows.Count == 0)
//             {
//                 await PublishAudit(0, request, ct);
//                 return (rows, 0);
//             }

//             // 2) Filter by current user's approver id (ServicePO module)
//             var currentUserId = _ipAddressService.GetUserId();

//             var poIds = rows.Select(r => r.Id)
//                             .Where(id => id > 0)
//                             .Distinct()
//                             .ToList();

//             var wfApprovers = await _workflowGrpcClient
//                 .GetApproverListAsync(MiscEnumEntity.ServicePO.ToString(), poIds);

//             var allowedPoIds = wfApprovers
//                 .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
//                             && int.TryParse(a.ApproverValue, out var parsed)
//                              && parsed == currentUserId
//                             )
//                 .Select(a => a.ModuleTransactionId)
//                 .ToHashSet();

//             rows = rows.Where(r => allowedPoIds.Contains(r.Id)).ToList();

//             if (rows.Count == 0)
//             {
//                 await PublishAudit(0, request, ct);
//                 return (rows, 0);
//             }

//             // 3) Build workflow map for remaining POs
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

//             // 4) Users → names
//             var users = await _usersAllGrpcClient.GetUserAllAsync();
//             var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

//             // 5) Vendors (fan-out)
//             var vendorIds = rows.Select(r => r.VendorId).Where(v => v > 0).Distinct().ToList();
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

//             // 6) Enrich rows (approver + vendor). Service lines already have all fields you need.
//             foreach (var g in rows)
//             {
//                 // workflow info
//                 if (wfByModuleId.TryGetValue(g.Id, out var wf))
//                 {
//                     if (wf.ApproverId.HasValue)
//                     {
//                         g.ApproverId = wf.ApproverId.Value; // == currentUserId by construction
//                         if (userLookup.TryGetValue(g.ApproverId.Value, out var approverName))
//                             g.ApproverName = approverName;
//                     }
//                     g.ApprovalRequestHeaderId = wf.ApprovalRequestId;
//                     g.IsEdit = wf.IsEdit;
//                 }

//                 // vendor info
//                 if (g.VendorId > 0 && vendorMap.TryGetValue(g.VendorId, out var v))
//                 {
//                     g.VendorName = v.Name;
//                     g.VendorEmail = v.Email;
//                     g.VendorMobile = v.Mobile;
//                 }

//                 // ensure Lines list not null
//                 g.Lines ??= new List<GetPOServicePendingDto>();
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

//         private Task PublishAudit(int count, GetPOServicePendingQuery q, CancellationToken ct)
//             => _mediator.Publish(new AuditLogsDomainEvent(
//                 actionDetail: "GetAll-Pending",
//                 actionCode: string.Empty,
//                 actionName: "ServicePO",
//                 details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
//                 module: "ServicePO"), ct);
//     }
// }