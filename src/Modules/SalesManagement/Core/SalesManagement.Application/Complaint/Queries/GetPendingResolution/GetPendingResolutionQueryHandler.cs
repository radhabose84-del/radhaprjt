using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Queries.GetPendingResolution
{
    public class GetPendingResolutionQueryHandler
        : IRequestHandler<GetPendingResolutionQuery, (List<PendingResolutionListDto> Items, int TotalCount)>
    {
        private readonly IComplaintQueryRepository _repo;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetPendingResolutionQueryHandler(
            IComplaintQueryRepository repo,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _repo = repo;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<(List<PendingResolutionListDto> Items, int TotalCount)> Handle(
            GetPendingResolutionQuery request, CancellationToken ct)
        {
            var (pending, _) = await _repo.GetPendingResolutionAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            if (pending.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (pending, 0);
            }

            var currentUserId = _ipAddressService.GetUserId();
            var moduleIds = pending.Select(r => r.ComplaintHeaderId).Distinct().ToList();

            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.ComplaintResolutionModuleTypeName, moduleIds);

            var allowedIds = wfApprovers
                .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
                            && int.TryParse(a.ApproverValue, out var parsed)
                            && parsed == currentUserId)
                .Select(a => a.ModuleTransactionId)
                .ToHashSet();

            pending = pending.Where(r => allowedIds.Contains(r.ComplaintHeaderId)).ToList();
            if (pending.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (pending, 0);
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

            var users = await _userLookup.GetAllUserAsync();
            var userNameMap = users.ToDictionary(u => u.UserId, u => u.UserName ?? string.Empty);

            foreach (var r in pending)
            {
                if (wfByModuleId.TryGetValue(r.ComplaintHeaderId, out var wf))
                {
                    if (wf.ApproverId.HasValue)
                    {
                        r.ApproverId = wf.ApproverId.Value;
                        if (userNameMap.TryGetValue(r.ApproverId, out var approverName))
                            r.ApproverName = approverName;
                    }
                    r.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                    r.IsEdit = wf.IsEdit;
                }
            }

            await PublishAudit(pending.Count, request, ct);
            return (pending, pending.Count);
        }

        private Task PublishAudit(int count, GetPendingResolutionQuery q, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll-Pending",
                actionCode: string.Empty,
                actionName: "ResolutionPending",
                details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}.",
                module: "Complaint"), ct);
    }
}
