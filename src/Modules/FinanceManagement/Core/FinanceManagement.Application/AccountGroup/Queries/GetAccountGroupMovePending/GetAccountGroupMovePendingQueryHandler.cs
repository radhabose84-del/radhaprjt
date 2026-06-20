using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupMovePending
{
    public class GetAccountGroupMovePendingQueryHandler : IRequestHandler<GetAccountGroupMovePendingQuery, ApiResponseDTO<List<AccountGroupMovePendingDto>>>
    {
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAccountGroupMovePendingQueryHandler(
            IAccountGroupQueryRepository queryRepository,
            IWorkflowLookup workflowLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _workflowLookup = workflowLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AccountGroupMovePendingDto>>> Handle(GetAccountGroupMovePendingQuery request, CancellationToken cancellationToken)
        {
            var (rows, _) = await _queryRepository.GetMovePendingAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            if (rows.Count > 0)
            {
                var currentUserId = _ipAddressService.GetUserId();
                var moduleIds = rows.Select(r => r.AccountGroupId).Distinct().ToList();

                // The engine returns the CURRENT step's pending approver per transaction — so filtering to the
                // logged-in user yields a correct multilevel inbox (FC sees L1-pending, CFO sees L2-pending).
                var wfApprovers = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.AccountGroupHierarchy, moduleIds);

                var byModule = wfApprovers
                    .GroupBy(a => a.ModuleTransactionId)
                    .ToDictionary(g => g.Key, g => g.First());

                rows = rows
                    .Where(r => byModule.TryGetValue(r.AccountGroupId, out var wf)
                                && int.TryParse(wf.ApproverValue, out var approverId)
                                && approverId == currentUserId)
                    .ToList();

                foreach (var r in rows)
                {
                    var wf = byModule[r.AccountGroupId];
                    r.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                    r.IsEdit = wf.IsEdit;
                    if (int.TryParse(wf.ApproverValue, out var approverId))
                        r.ApproverId = approverId;
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll-Pending",
                actionCode: "GetAccountGroupMovePendingQuery",
                actionName: rows.Count.ToString(),
                details: $"Account Group Move pending inbox fetched ({rows.Count} awaiting the current approver).",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AccountGroupMovePendingDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = rows,
                TotalCount = rows.Count,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
