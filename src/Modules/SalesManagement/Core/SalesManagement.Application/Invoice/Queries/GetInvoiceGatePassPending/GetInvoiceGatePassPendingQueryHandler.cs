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
        private readonly IUOMLookup _uomLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMediator _mediator;

        public GetInvoiceGatePassPendingQueryHandler(
            IInvoiceQueryRepository repo,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IFinancialYearLookup financialYearLookup,
            IMediator mediator)
        {
            _repo = repo;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _financialYearLookup = financialYearLookup;
            _mediator = mediator;
        }

        public async Task<List<GetInvoiceGatePassPendingDto>> Handle(
            GetInvoiceGatePassPendingQuery request, CancellationToken ct)
        {
            var pending = await _repo.GetInvoiceGatePassPendingAsync(request.VehicleNo);

            if (pending.Count == 0)
            {
                await PublishAudit(0, ct);
                return pending;
            }

            // Collect IDs for cross-module enrichment
            var partyIds = pending.Select(r => r.PartyId).Distinct().ToList();
            var agentIds = pending.Where(r => r.AgentId.HasValue).Select(r => r.AgentId!.Value).Distinct().ToList();
            var allPartyIds = partyIds.Union(agentIds).ToList();
            var finYearIds = pending.Select(r => r.FinancialYearId).Distinct().ToList();

            var allDetails = pending
                .SelectMany(r => r.InvoiceDetails ?? new List<GetInvoiceGatePassPendingDto.GetInvoiceGatePassPendingDetailDto>())
                .ToList();

            var itemIds = allDetails
                .Select(d => d.ItemId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var uomIds = allDetails
                .Where(d => d.UOMId.HasValue)
                .Select(d => d.UOMId!.Value)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            // Parallel cross-module lookups
            var partyTask = _partyLookup.GetByIdsAsync(allPartyIds, ct);
            var unitTask = _unitLookup.GetAllUnitAsync();
            var finYearTask = _financialYearLookup.GetByIdsAsync(finYearIds, ct);
            var itemTask = itemIds.Count > 0
                ? _itemLookup.GetByIdsAsync(itemIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());
            var uomTask = uomIds.Count > 0
                ? _uomLookup.GetByIdsAsync(uomIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>());

            await Task.WhenAll(partyTask, unitTask, finYearTask, itemTask, uomTask);

            // Build lookup dictionaries
            var partyDict = (await partyTask).ToDictionary(p => p.Id, p => p.PartyName);
            var unitDict = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
            var finYearDict = (await finYearTask).ToDictionary(f => f.FinancialYearId, f => f.FinancialYearName);

            var itemNameMap = (await itemTask)
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g =>
                {
                    var it = g.First();
                    return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName : (it.ItemCode ?? string.Empty);
                });

            var uomMap = (await uomTask)
                .GroupBy(u => u.Id)
                .ToDictionary(g => g.Key, g =>
                {
                    var u = g.First();
                    return !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName : (u.Code ?? u.Id.ToString());
                });

            // Enrich
            foreach (var r in pending)
            {
                if (partyDict.TryGetValue(r.PartyId, out var partyName))
                    r.PartyName = partyName;

                if (r.AgentId.HasValue && partyDict.TryGetValue(r.AgentId.Value, out var agentName))
                    r.AgentName = agentName;

                if (unitDict.TryGetValue(r.UnitId, out var unitName))
                    r.UnitName = unitName;

                if (finYearDict.TryGetValue(r.FinancialYearId, out var finYearName))
                    r.FinancialYearName = finYearName;

                if (r.InvoiceDetails == null) continue;

                foreach (var detail in r.InvoiceDetails)
                {
                    if (itemNameMap.TryGetValue(detail.ItemId, out var iName))
                        detail.ItemName = iName;

                    if (detail.UOMId.HasValue && uomMap.TryGetValue(detail.UOMId.Value, out var uName))
                        detail.UOMName = uName;
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
