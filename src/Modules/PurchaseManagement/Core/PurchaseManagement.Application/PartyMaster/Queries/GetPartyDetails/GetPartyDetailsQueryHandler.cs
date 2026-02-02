using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPartyMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails
{
    public class GetPartyDetailsQueryHandler : IRequestHandler<GetPartyDetailsQuery, List<PartyMasterDTO>>
    {
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetPartyDetailsQueryHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<PartyMasterDTO>> Handle(GetPartyDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _ipartyMasterQueryRepository.GetPartyMasters(request.OldunitCode,request.SearchPattern);
            var partyGroups = _mapper.Map<List<PartyMasterDTO>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetPartyDetailsQuery",        
                    actionName: partyGroups.Count.ToString(),
                    details: $"Party details was fetched.",
                    module:"PartyMaster"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return partyGroups;
        }
    }
}