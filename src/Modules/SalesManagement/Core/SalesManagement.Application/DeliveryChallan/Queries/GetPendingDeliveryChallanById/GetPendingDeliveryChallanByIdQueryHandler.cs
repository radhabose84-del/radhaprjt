using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallanById
{
    public class GetPendingDeliveryChallanByIdQueryHandler
        : IRequestHandler<GetPendingDeliveryChallanByIdQuery, PendingDeliveryChallanByIdDto?>
    {
        private readonly IDeliveryChallanQueryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetPendingDeliveryChallanByIdQueryHandler(
            IDeliveryChallanQueryRepository repo,
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

        public async Task<PendingDeliveryChallanByIdDto?> Handle(
            GetPendingDeliveryChallanByIdQuery request, CancellationToken ct)
        {
            // 1 — Fetch the delivery challan only if it has Pending status
            var header = await _repo.GetPendingByIdAsync(request.Id);
            if (header == null)
                return null;

            // Map to PendingDeliveryChallanByIdDto
            var result = _mapper.Map<PendingDeliveryChallanByIdDto>(header);

            // 2 — Workflow enrichment
            var currentUserId = _ipAddressService.GetUserId();
            var moduleIds = new List<int> { result.Id };

            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.TransactionTypeStodc, moduleIds);

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
                actionDetail: "GetPendingDeliveryChallanById",
                actionCode: "GetPendingDeliveryChallanById",
                actionName: result.Id.ToString(),
                details: $"Delivery Challan pending details {result.Id} was fetched.",
                module: "DeliveryChallan");
            await _mediator.Publish(evt, ct);

            return result;
        }
    }
}
