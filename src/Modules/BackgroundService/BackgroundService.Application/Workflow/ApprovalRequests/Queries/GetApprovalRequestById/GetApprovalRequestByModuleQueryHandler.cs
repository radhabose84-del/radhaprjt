using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById
{
    public class GetApprovalRequestByModuleQueryHandler : IRequestHandler<GetApprovalRequestByModuleQuery, List<ApprovalRequestWithLinesDto>>
    {
        private readonly IApprovalRequestQuery _repository;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ip;

        private readonly IApprovalRequestQuery _approvalRequestQuery;

        public GetApprovalRequestByModuleQueryHandler(IApprovalRequestQuery repository, IMapper mapper, IIPAddressService ip, IApprovalRequestQuery approvalRequestQuery)
        {
            _repository = repository;
            _mapper = mapper;
            _ip = ip;
            _approvalRequestQuery = approvalRequestQuery;
        }

        public async Task<List<ApprovalRequestWithLinesDto>> Handle(
            GetApprovalRequestByModuleQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _ip.GetUserId();

            // 1️⃣ Get all approval requests (header + lines) for this module + workflow type
            var result = await _repository
                .GetByModuleAsync(request.ModuleTransactionId, request.WorkflowTypeId);

            if (result == null || result.Count == 0)
                return new List<ApprovalRequestWithLinesDto>();

            var filtered = result
                .Where(a =>
                    !string.IsNullOrWhiteSpace(a.ApproverValue) &&
                    int.TryParse(a.ApproverValue, out var parsed) &&
                    parsed == currentUserId)
                .ToList();

           

            return filtered;
        }
        
        //  public async Task<List<ApprovalRequestWithLinesDto>> Handle(
        //     GetApprovalRequestByModuleQuery request,
        //     CancellationToken cancellationToken)
        // {
        //     var currentUserId = _ip.GetUserId();

        //     // 1️⃣ Check workflow approver for this module + workflow type
        //     var approverItems = await IApprovalRequestGrpcQuery.GetApproverListByWorkFlowTypeAsync(
        //         // IMPORTANT: WorkflowType name as string (same as you pass to gRPC elsewhere)
        //         request.,                           // or MiscEnumEntity.PurchaseIndent.ToString()
        //         new[] { request.ModuleTransactionId }           // single module id
        //     );

        //     // 2️⃣ Is current user mapped as approver?
        //     var isCurrentUserApprover = approverItems.Any(a =>
        //         a.ApproverBinding == "USER" &&
        //         !string.IsNullOrWhiteSpace(a.ApproverValue) &&
        //         int.TryParse(a.ApproverValue, out var parsed) &&
        //         parsed == currentUserId);

        //     // If not mapped → return empty (no access to this approval detail)
        //     if (!isCurrentUserApprover)
        //         return new List<ApprovalRequestWithLinesDto>();

        //     // 3️⃣ Load full approval + lines for this module
        //     var result = await _repository
        //         .GetByModuleAsync(request.ModuleTransactionId, request.WorkflowTypeId);

        //     // (Optional) If your DTO has ApproverId / IsEdit etc.,
        //     // you can enrich the result from approverItems.First()
        //     // var first = approverItems.First();
        //     // foreach (var r in result)
        //     // {
        //     //     r.ApproverId = int.TryParse(first.ApproverValue, out var parsed) ? parsed : (int?)null;
        //     //     r.IsEdit = first.IsEdit;
        //     // }

        //     return result;
        // }

        // public async Task<List<ApprovalRequestWithLinesDto>> Handle(        GetApprovalRequestByModuleQuery request,        CancellationToken cancellationToken)
        // {
        //     return await _repository
        //         .GetByModuleAsync(request.ModuleTransactionId, request.WorkflowTypeId);
        // }


        // public async Task<List<ApprovalRequestWithLinesDto>> Handle(    GetApprovalRequestByModuleQuery request,    CancellationToken cancellationToken)
        // {
        //     var currentUserId = _ip.GetUserId();

        //     var result = await _repository
        //         .GetByModuleAsync(request.ModuleTransactionId, request.WorkflowTypeId);

        //     // Filter: only records where ApproverBinding = USER and ApproverValue = current user
        //     var filtered = result
        //         .Where(a =>
        //             a.ApproverBinding == "USER" &&
        //             !string.IsNullOrWhiteSpace(a.ApproverValue) &&
        //             int.TryParse(a.ApproverValue, out var parsed) &&
        //             parsed == currentUserId)
        //         .ToList();

        //     return filtered;
        // }
    }
}