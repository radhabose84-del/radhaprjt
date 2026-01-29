using MediatR;
using Core.Application.State.Queries.GetStates;
using AutoMapper;
using Core.Application.Common.Interfaces.IState;
using Core.Domain.Events;
using Core.Application.Common.HttpResponse;
using FluentValidation;

namespace Core.Application.State.Queries.GetStateById
{
    public class GetStateByIdQueryHandler : IRequestHandler<GetStateByIdQuery, StateDto>
    {
        private readonly IStateQueryRepository _stateRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        public GetStateByIdQueryHandler(IStateQueryRepository stateRepository, IMapper mapper, IMediator mediator)
        {
            _stateRepository = stateRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<StateDto> Handle(GetStateByIdQuery request, CancellationToken cancellationToken)
        {            
            var state = await _stateRepository.GetByIdAsync(request.Id);
            var stateDto = _mapper.Map<StateDto>(state);
            if (state is null)
            { 
                throw new ValidationException("Country with ID {request.Id} not found.");               
                
            }            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: stateDto.StateCode ?? string.Empty,        
                actionName: stateDto.StateName ?? string.Empty,                                
                details: $"Get StateId: {request.Id}. details was fetched.",
                module:"State"
            );
            await _mediator.Publish(domainEvent, cancellationToken);            
            return stateDto;   
        }          
    }
}
