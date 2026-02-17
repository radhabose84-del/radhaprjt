using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Dtos.Lookups.Inventory;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails
{
    public class GetGrnPendingDetailsQueryHandler : IRequestHandler<GetGrnPendingDetailsQuery, List<GetGrnPendingDetailsDto>>
    {
        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemPurchaseToleranceLookup _itemPurchaseToleranceLookup;
        private readonly IPartyLookup _partyLookup;


        public GetGrnPendingDetailsQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IItemPurchaseToleranceLookup itemPurchaseToleranceLookup, IPartyLookup partyLookup)
        {
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _itemPurchaseToleranceLookup = itemPurchaseToleranceLookup;
            _partyLookup = partyLookup;

        }

        public async Task<List<GetGrnPendingDetailsDto>> Handle(    GetGrnPendingDetailsQuery request,    CancellationToken cancellationToken)
    
        {
            // 1) Fetch pending details
            var result   = await _iGrnEntryQueryRepository.GetPendingGateEntriesForGrnAsync(request.GrnId ,request.IsGrnGenerated, request.IsQcGenerated);
            var pending  = _mapper.Map<List<GetGrnPendingDetailsDto>>(result);
            var partyIds = pending.Select(p => p.PartyId).Where(id => id > 0).Distinct().ToList();
            IReadOnlyList<Contracts.Dtos.Lookups.Party.PartyLookupDto> partyLookupResult = Array.Empty<Contracts.Dtos.Lookups.Party.PartyLookupDto>();
            if (partyIds.Any())
            {
                partyLookupResult = await _partyLookup.GetByIdsAsync(partyIds, cancellationToken);
            }
            var partyMap = partyLookupResult.ToDictionary(p => p.Id, p => p);

            foreach (var po in pending)
            {
                if (po.PartyId > 0 && partyMap.TryGetValue(po.PartyId, out var partyDetails))
                {
                    po.PartyName = partyDetails.PartyName;
                }
            }

            // 2) Gather unique ItemIds (no null-coalescing on different list types)
            var itemIdSet = new HashSet<int>();
            foreach (var header in pending)
            {
                if (header?.GrnDetails == null) continue;
                foreach (var d in header.GrnDetails)
                    if (d.ItemId > 0) itemIdSet.Add(d.ItemId);
            }

             // 3) Batch call to Inventory (one network hop)
            Dictionary<int, ItemPurchaseToleranceLookupDto> tolMap = new();
            if (itemIdSet.Count > 0)
            {
                // If your IInventoryGrpcClient supports the batch overload:
                var tolList = await _itemPurchaseToleranceLookup.GetByIdsAsync(itemIdSet, cancellationToken);

          

                tolMap = tolList.GroupBy(t => t.ItemId).ToDictionary(g => g.Key, g => g.First());
            }

            // 4) Enrich all GRN details from the map
            foreach (var header in pending)
            {
                if (header?.GrnDetails == null) continue;

                foreach (var detail in header.GrnDetails)
                {
                    if (!tolMap.TryGetValue(detail.ItemId, out var tol) || tol == null) continue;

                    if (!string.IsNullOrWhiteSpace(tol.ItemName))
                        detail.ItemName = tol.ItemName;

                    if (!string.IsNullOrWhiteSpace(tol.UOMName))
                        detail.UOMName = tol.UOMName;

                    // If your detail tolerances are decimal?:

                    detail.LowerTolerance = tol.LowerTolerance;
                    detail.UpperTolerance = tol.UpperTolerance;
                    detail.ItemCode = tol.ItemCode;
                
                   
                }
            }

            // 5) Domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGrnPendingDetailsQuery",
                actionName: pending.Count.ToString(),
                details: "Pending PO details were fetched.",
                module: "GRNEntry");

            await _mediator.Publish(domainEvent, cancellationToken);
            return pending;

        }
    }
}
