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
    }
}