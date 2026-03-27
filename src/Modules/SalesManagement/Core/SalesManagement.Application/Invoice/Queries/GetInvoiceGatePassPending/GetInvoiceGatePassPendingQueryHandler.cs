using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending
{
    public sealed class GetInvoiceGatePassPendingQueryHandler
        : IRequestHandler<GetInvoiceGatePassPendingQuery, List<GetInvoiceGatePassPendingDto>>
    {
        private readonly IInvoiceQueryRepository _repo;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IMediator _mediator;

        public GetInvoiceGatePassPendingQueryHandler(
            IInvoiceQueryRepository repo,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IMediator mediator)
        {
            _repo = repo;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _mediator = mediator;
        }

        public async Task<List<GetInvoiceGatePassPendingDto>> Handle(
            GetInvoiceGatePassPendingQuery request, CancellationToken ct)
        {
            var pending = await _repo.GetInvoiceGatePassPendingAsync();

            if (pending.Count == 0)
            {
                await PublishAudit(0, ct);
                return pending;
            }

            // Cross-module enrichment
            var partyIds = pending.Select(r => r.PartyId).Distinct().ToList();

            var partyTask = _partyLookup.GetByIdsAsync(partyIds, ct);
            var unitTask = _unitLookup.GetAllUnitAsync();

            await Task.WhenAll(partyTask, unitTask);

            var partyDict = (await partyTask).ToDictionary(p => p.Id, p => p.PartyName);
            var unitDict = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var r in pending)
            {
                if (partyDict.TryGetValue(r.PartyId, out var partyName))
                    r.PartyName = partyName;

                if (unitDict.TryGetValue(r.UnitId, out var unitName))
                    r.UnitName = unitName;
            }

            await PublishAudit(pending.Count, ct);
            return pending;
        }

        private Task PublishAudit(int count, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll-GatePassPending",
                    actionCode: string.Empty,
                    actionName: "InvoiceGatePassPending",
                    details: $"Fetched {count} invoices pending for gate pass.",
                    module: "Invoice"), ct);
    }
}
