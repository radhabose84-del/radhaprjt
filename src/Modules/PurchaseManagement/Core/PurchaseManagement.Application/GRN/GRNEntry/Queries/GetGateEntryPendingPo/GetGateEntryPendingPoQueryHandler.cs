using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Party;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo
{
    public class GetGateEntryPendingPoQueryHandler : IRequestHandler<GetGateEntryPendingPoQuery, List<GetGateEntryPendingPoDto>>
    {
        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPartyLookup _partyLookup;

        public GetGateEntryPendingPoQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator, IPartyLookup partyLookup)
        {
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _partyLookup = partyLookup;
        }

        public async Task<List<GetGateEntryPendingPoDto>> Handle(GetGateEntryPendingPoQuery request, CancellationToken cancellationToken)
        {
             var result = await _iGrnEntryQueryRepository.GetPendingPoAsync(request.PartyId);
            var pendingpoIds = _mapper.Map<List<GetGateEntryPendingPoDto>>(result);
            var partyIds = pendingpoIds.Select(p => p.PartyId).Where(id => id > 0).Distinct().ToList();
            IReadOnlyList<Contracts.Dtos.Lookups.Party.PartyLookupDto> partyLookupResult = Array.Empty<Contracts.Dtos.Lookups.Party.PartyLookupDto>();
            if (partyIds.Any())
            {
                partyLookupResult = await _partyLookup.GetByIdsAsync(partyIds, cancellationToken);
            }
            var partyMap = partyLookupResult.ToDictionary(p => p.Id, p => p);
            foreach (var po in pendingpoIds)
            {
                if (po.PartyId > 0 && partyMap.TryGetValue(po.PartyId, out var party))
                    po.PartyName = party.PartyName;
            }
            
             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetGateEntryPendingPoQuery",        
                    actionName: pendingpoIds.Count.ToString(),
                    details: $"Pending PO details was fetched.",
                    module:"GRNEntry"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return pendingpoIds;
        }
    }
}
