#nullable disable
using AutoMapper;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById
{
    public class GetPartyGroupByIdQueryHandler : IRequestHandler<GetPartyGroupByIdQuery, PartyGroupByIdDto>
    {
        private readonly IPartyGroupQueryRepository _ipartygroupQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetPartyGroupByIdQueryHandler(IPartyGroupQueryRepository ipartygroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ipartygroupQueryRepository = ipartygroupQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }       

        public async Task<PartyGroupByIdDto> Handle(GetPartyGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _ipartygroupQueryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null; // No CS8603 warning because return type is now nullable

            var partymaster = _mapper.Map<PartyGroupByIdDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPartyGroupByIdQuery",
                actionName: partymaster.Id.ToString(),
                details: $"PartyGroup details {partymaster.Id} was fetched.",
                module: "PartyGroup"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return partymaster;
        }

    }
}