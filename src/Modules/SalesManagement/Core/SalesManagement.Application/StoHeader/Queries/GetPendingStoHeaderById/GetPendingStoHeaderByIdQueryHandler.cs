using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Queries.GetPendingStoHeaderById
{
    public class GetPendingStoHeaderByIdQueryHandler
        : IRequestHandler<GetPendingStoHeaderByIdQuery, PendingStoHeaderByIdDto?>
    {
        private readonly IStoHeaderQueryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetPendingStoHeaderByIdQueryHandler(
            IStoHeaderQueryRepository repo,
            IMapper mapper,
            IMediator mediator,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService)
        {
            _repo = repo;
            _mapper = mapper;
            _mediator = mediator;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<PendingStoHeaderByIdDto?> Handle(
            GetPendingStoHeaderByIdQuery request, CancellationToken ct)
        {
            // 1 — Fetch the STO header only if it has Pending status
            var header = await _repo.GetPendingByIdAsync(request.Id);
            if (header == null)
                return null;

            // Map to PendingStoHeaderByIdDto
            var result = _mapper.Map<PendingStoHeaderByIdDto>(header);

            // 2 — Workflow enrichment
            var currentUserId = _ipAddressService.GetUserId();
            var moduleIds = new List<int> { result.Id };

            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.TransactionTypeSto, moduleIds);

            var wfByTransaction = wfApprovers
                .GroupBy(x => x.ModuleTransactionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (wfByTransaction.TryGetValue(result.Id, out var approverEntries))
            {
                var currentUserEntry = approverEntries
                    .FirstOrDefault(a => a.ApproverValue == currentUserId.ToString())
                    ?? approverEntries.First();

                result.ApprovalRequestHeaderId = currentUserEntry.ApprovalRequestId;
                result.ApproverId = Convert.ToInt32(currentUserEntry.ApproverValue);
            }

            // 3 — Approver name
            var users = await _userLookup.GetAllUserAsync();
            var userNameMap = users.ToDictionary(u => u.UserId, u => u.UserName ?? string.Empty);

            if (userNameMap.TryGetValue(result.ApproverId, out var approverName))
            {
                result.ApproverName = approverName;
            }

            // 4 — Audit log
            var evt = new AuditLogsDomainEvent(
                actionDetail: "GetPendingStoHeaderById",
                actionCode: "GetPendingStoHeaderById",
                actionName: result.Id.ToString(),
                details: $"STO pending details {result.Id} was fetched.",
                module: "StoHeader");
            await _mediator.Publish(evt, ct);

            return result;
        }
    }
}
