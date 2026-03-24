using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderById
{
    public class GetPendingSalesOrderByIdQueryHandler
        : IRequestHandler<GetPendingSalesOrderByIdQuery, PendingSalesOrderByIdDto?>
    {
        private readonly ISalesOrderQueryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IHSNLookup _hsnLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetPendingSalesOrderByIdQueryHandler(
            ISalesOrderQueryRepository repo,
            IMapper mapper,
            IMediator mediator,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IPaymentTermLookup paymentTermLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IUOMLookup uomLookup,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService)
        {
            _repo = repo;
            _mapper = mapper;
            _mediator = mediator;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _paymentTermLookup = paymentTermLookup;
            _itemLookup = itemLookup;
            _hsnLookup = hsnLookup;
            _uomLookup = uomLookup;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<PendingSalesOrderByIdDto?> Handle(
            GetPendingSalesOrderByIdQuery request, CancellationToken ct)
        {
            // 1 — Fetch the full sales order (reuse existing GetByIdAsync)
            var header = await _repo.GetByIdAsync(request.Id);
            if (header == null)
                return null;

            // Map to PendingSalesOrderByIdDto
            var result = _mapper.Map<PendingSalesOrderByIdDto>(header);

            // 2 — Workflow enrichment
            var currentUserId = _ipAddressService.GetUserId();
            var moduleIds = new List<int> { result.Id };

            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.TransactionTypeSalesOrder, moduleIds);

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
                actionDetail: "GetPendingSalesOrderById",
                actionCode: "GetPendingSalesOrderById",
                actionName: result.Id.ToString(),
                details: $"Sales Order pending details {result.Id} was fetched.",
                module: "SalesOrder");
            await _mediator.Publish(evt, ct);

            return result;
        }
    }
}
