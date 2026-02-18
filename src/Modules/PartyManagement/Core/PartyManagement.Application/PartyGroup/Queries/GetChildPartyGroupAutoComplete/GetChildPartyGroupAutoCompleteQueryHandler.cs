#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Queries.GetChildPartyGroupAutoComplete
{
    public class GetChildPartyGroupAutoCompleteQueryHandler : IRequestHandler<GetChildPartyGroupAutoCompleteQuery, List<PartyGroupAutoCompleteDto>>
    {
        private readonly IPartyGroupQueryRepository _ipartygroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetChildPartyGroupAutoCompleteQueryHandler(IPartyGroupQueryRepository ipartygroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ipartygroupQueryRepository = ipartygroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<PartyGroupAutoCompleteDto>> Handle(GetChildPartyGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _ipartygroupQueryRepository.GetParentPartyGroups(request.SearchPattern);
            var partyGroups = _mapper.Map<List<PartyGroupAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetChildPartyGroupAutoCompleteQuery",        
                    actionName: partyGroups.Count.ToString(),
                    details: $"PartyGroup details was fetched.",
                    module:"PartyGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return partyGroups;
        }
    }
}