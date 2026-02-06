using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad
{
    public class GetPartyGroupLoadQueryHandler : IRequestHandler<GetPartyGroupLoadQuery, List<PartyGroupLoadDto>>
    {
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPartyGroupLoadQueryHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

       public async Task<List<PartyGroupLoadDto>> Handle(GetPartyGroupLoadQuery request, CancellationToken cancellationToken)
        {
            // Fetch data from repository
            var result = await _ipartyMasterQueryRepository.GetPartyGroupsAsync(request.GroupTypeIds);

            // Map to DTOs (if needed — if repository already returns DTOs, you can skip this)
            var partyGroups = _mapper.Map<List<PartyGroupLoadDto>>(result);

            // Domain Event logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPartyGroupLoadQuery",
                actionName: partyGroups.Count.ToString(),
                details: $"PartyGroup details were fetched for GroupTypeIds: {string.Join(",", request.GroupTypeIds)}",
                module: "PartyGroup"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return partyGroups;
        }

        
    }
}