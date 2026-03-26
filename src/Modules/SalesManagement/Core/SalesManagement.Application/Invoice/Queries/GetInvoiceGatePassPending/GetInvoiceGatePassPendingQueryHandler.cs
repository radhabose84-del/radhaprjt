using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
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
        private readonly IItemLookup _itemLookup;
        private readonly IMediator _mediator;

        public GetInvoiceGatePassPendingQueryHandler(
            IInvoiceQueryRepository repo,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IMediator mediator)
        {
            _repo = repo;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _itemLookup = itemLookup;
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
            var itemIds = pending.SelectMany(r => r.InvoiceDetails).Select(d => d.ItemId).Where(id => id > 0).Distinct().ToList();

            var partyTask = _partyLookup.GetByIdsAsync(partyIds, ct);
            var unitTask = _unitLookup.GetAllUnitAsync();
            var itemTask = itemIds.Count > 0
                ? _itemLookup.GetByIdsAsync(itemIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());

            await Task.WhenAll(partyTask, unitTask, itemTask);

            var partyDict = (await partyTask).ToDictionary(p => p.Id, p => p.PartyName);
            var unitDict = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
            var itemDict = (await itemTask).GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().ItemName ?? g.First().ItemCode ?? string.Empty);

            foreach (var r in pending)
            {
                if (partyDict.TryGetValue(r.PartyId, out var partyName))
                    r.PartyName = partyName;

                if (unitDict.TryGetValue(r.UnitId, out var unitName))
                    r.UnitName = unitName;

                foreach (var detail in r.InvoiceDetails)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemName))
                        detail.ItemName = itemName;
                }
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
