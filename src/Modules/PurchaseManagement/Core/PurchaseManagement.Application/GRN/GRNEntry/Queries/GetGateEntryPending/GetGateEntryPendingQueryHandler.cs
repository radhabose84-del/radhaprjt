using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending
{
    public class GetGateEntryPendingQueryHandler : IRequestHandler<GetGateEntryPendingQuery, List<GetGateEntryPendingDto>>
    {
        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
         public GetGateEntryPendingQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }


        public async Task<List<GetGateEntryPendingDto>> Handle(GetGateEntryPendingQuery request, CancellationToken cancellationToken)
        {
             var result = await _iGrnEntryQueryRepository.GetPendingPoGateAsync(request.PartyId,request.PoId);
            var pendingpoIds = _mapper.Map<List<GetGateEntryPendingDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetGateEntryPendingQuery",        
                    actionName: pendingpoIds.Count.ToString(),
                    details: $"Pending PO details was fetched.",
                    module:"GRNEntry"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return pendingpoIds;
        }
    }
}