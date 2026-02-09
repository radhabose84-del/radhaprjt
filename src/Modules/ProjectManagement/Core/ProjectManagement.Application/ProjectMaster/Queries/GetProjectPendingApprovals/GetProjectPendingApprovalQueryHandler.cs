using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IUser;
using Contracts.Interfaces.External.IWorkflow;
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
        private readonly IWorkflowGrpcClient _workflowGrpc;
        private readonly IUsersAllGrpcClient _usersGrpc;
        private readonly IDepartmentAllGrpcClient _deptGrpc;
        private readonly IIPAddressService _ip;
        private readonly IMapper _mapper;

        public GetProjectPendingApprovalQueryHandler(
          IProjectMasterQueryRepository repo,
          IMiscMasterQueryRepository misc,
          IWorkflowGrpcClient workflowGrpc,
          IUsersAllGrpcClient usersGrpc,
          IDepartmentAllGrpcClient deptGrpc,
          IIPAddressService ip,
          IMapper mapper
          )
        {
            _repo = repo;
            _misc = misc;
            _workflowGrpc = workflowGrpc;
            _usersGrpc = usersGrpc;
            _deptGrpc = deptGrpc;
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
            var wfApprovers = await _workflowGrpc.GetApproverListAsync(
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

            var users = await _usersGrpc.GetUserAllAsync();
            var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

            var deps = await _deptGrpc.GetDepartmentAllAsync();
            var depLookup = (deps as IEnumerable<dynamic> ?? Enumerable.Empty<dynamic>())
                .ToDictionary(d => (int)d.DepartmentId, d => (string?)d.DepartmentName ?? string.Empty);

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
        
        //  public async Task<(List<GetProjectPendingApprovalDto> Items, int TotalCount)> Handle(
        //     GetProjectPendingApprovalQuery request, CancellationToken ct)
        // {
        //     var unitId = _ip.GetUnitId();
        //     var currentUserId = _ip.GetUserId();

        //     var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

        //     var (rows, _) = await _repo.GetProjectPendingAsync(
        //         request.PageNumber ?? 1,
        //         request.PageSize ?? 15,
        //         request.SearchTerm,
        //         request.ProjectId,
        //         request.DepartmentId,
        //         request.ProjectTypeId,
        //         request.BudgetYearId,
        //         unitId,
        //         pending.Id,
        //         ct);

        //     // rows ??= new List<GetProjectPendingApprovalDto>();
        //          var PendingDtos = _mapper.Map<List<GetProjectPendingApprovalDto>>(rows);


        //     var projectIds = PendingDtos.Select(r => r.Id).Distinct().ToList();
        //     // var projectIds = PendingDtos.Select(d => d.Id).ToList();

        //     var wfApprovers = await _workflowGrpc.GetApproverListAsync(
        //         MiscEnumEntity.ProjectMaster,projectIds);

        //     var allowedProjectIds = wfApprovers
        //         .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
        //                     && int.TryParse(a.ApproverValue, out var parsed)
        //                     && parsed == currentUserId)
        //         .Select(a => a.ModuleTransactionId)
        //         .ToHashSet();

        //     rows = rows.Where(r => allowedProjectIds.Contains(r.Id)).ToList();

        //     if (rows.Count == 0)
        //         return (rows, 0);

        //     var wfByProjectId = wfApprovers
        //         .Where(a => allowedProjectIds.Contains(a.ModuleTransactionId))
        //         .GroupBy(a => a.ModuleTransactionId)
        //         .ToDictionary(g => g.Key, g =>
        //         {
        //             var first = g.First();
        //             int? approverId = null;

        //             if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
        //                 int.TryParse(first.ApproverValue, out var parsed))
        //                 approverId = parsed;

        //             return new { ApproverId = approverId, ApprovalRequestId = first.ApprovalRequestId };
        //         });

        //     var users = await _usersGrpc.GetUserAllAsync();
        //     var userLookup = users.ToDictionary(u => u.UserId, u => u.UserName);

        //     var deps = await _deptGrpc.GetDepartmentAllAsync();
        //     var depLookup = (deps as IEnumerable<dynamic> ?? Enumerable.Empty<dynamic>())
        //         .ToDictionary(d => (int)d.DepartmentId, d => (string?)d.DepartmentName ?? string.Empty);

        //     foreach (var r in rows)
        //     {
        //         if (r.DepartmentId.HasValue &&
        //             depLookup.TryGetValue(r.DepartmentId.Value, out var dn))
        //         {
        //             r.DepartmentName = dn;
        //         }

        //         if (wfByProjectId.TryGetValue(r.Id, out var wf))
        //         {
        //             r.ApproverId = wf.ApproverId;
        //             r.ApprovalRequestHeaderId = wf.ApprovalRequestId;

        //             if (wf.ApproverId.HasValue &&
        //                 userLookup.TryGetValue(wf.ApproverId.Value, out var an))
        //             {
        //                 r.ApproverName = an;
        //             }
        //         }
        //     }

        //     return (rows, rows.Count);
        // }
    }
}