using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Domain.Common;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals
{
    public class GetProjectPendingApprovalQueryHandler : IRequestHandler<GetProjectPendingApprovalQuery, (List<GetProjectPendingApprovalDto> Items, int TotalCount)>
    {

        private readonly IProjectMasterQueryRepository _repo;
        private readonly IMiscMasterQueryRepository _misc;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IIPAddressService _ip;
        private readonly IMapper _mapper;

        public GetProjectPendingApprovalQueryHandler(
          IProjectMasterQueryRepository repo,
          IMiscMasterQueryRepository misc,
          IWorkflowLookup workflowLookup,
          IUserLookup userLookup,
          IDepartmentLookup departmentLookup,
          IIPAddressService ip,
          IMapper mapper
          )
        {
            _repo = repo;
            _misc = misc;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _departmentLookup = departmentLookup;
            _ip = ip;
            _mapper = mapper;
        }
        
        public async Task<(List<GetProjectPendingApprovalDto> Items, int TotalCount)> Handle(
    GetProjectPendingApprovalQuery request, CancellationToken ct)
        {
            var unitId = _ip.GetUnitId();
            var currentUserId = _ip.GetUserId();

            var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

            var (rows, _) = await _repo.GetProjectPendingAsync(
                request.PageNumber ?? 1,
                request.PageSize ?? 15,
                request.SearchTerm,
                request.ProjectId,
                request.DepartmentId,
                request.ProjectTypeId,
                request.BudgetYearId,
                unitId,
                pending.Id,
                ct);

            rows ??= new List<GetProjectPendingApprovalDto>();

            if (rows.Count == 0)
                return (rows, 0);

            var projectIds = rows.Select(r => r.Id).Where(x => x > 0).Distinct().ToList();

            // IMPORTANT: pass module name as string
            var wfApprovers = await _workflowLookup.GetApproverListAsync(
                MiscEnumEntity.ProjectMaster.ToString(),
                projectIds);

            var allowedIds = wfApprovers
                    .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
                                && int.TryParse(a.ApproverValue, out var parsed)
                                && parsed == currentUserId)
                .Select(a => a.ModuleTransactionId)
                .ToHashSet();

            rows = rows.Where(r => allowedIds.Contains(r.Id)).ToList();

            if (rows.Count == 0)
                return (rows, 0);

            var wfById = wfApprovers
                .Where(a => allowedIds.Contains(a.ModuleTransactionId))
                .GroupBy(a => a.ModuleTransactionId)
                .ToDictionary(g => g.Key, g =>
                {
                    var first = g.First();
                    int? approverId = null;

                    if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
                        int.TryParse(first.ApproverValue, out var parsed))
                        approverId = parsed;

                    return new { ApproverId = approverId, ApprovalRequestId = first.ApprovalRequestId };
                });

            var users = await _userLookup.GetAllUserAsync();
            var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

            var deps = await _departmentLookup.GetAllDepartmentAsync();
            var depLookup = deps.ToDictionary(d => d.DepartmentId, d => d.DepartmentName ?? string.Empty);

            foreach (var r in rows)
            {
                if (r.DepartmentId.HasValue && depLookup.TryGetValue(r.DepartmentId.Value, out var deptName))
                    r.DepartmentName = deptName;

                if (wfById.TryGetValue(r.Id, out var wf))
                {
                    r.ApproverId = wf.ApproverId;
                    r.ApprovalRequestHeaderId = wf.ApprovalRequestId;

                    if (wf.ApproverId.HasValue && userLookup.TryGetValue(wf.ApproverId.Value, out var approverName))
                        r.ApproverName = approverName;
                }
            }

            // same as your ServicePO approach: after approver-filter, return rows.Count
            return (rows, rows.Count);
        }       
     
    }
}
