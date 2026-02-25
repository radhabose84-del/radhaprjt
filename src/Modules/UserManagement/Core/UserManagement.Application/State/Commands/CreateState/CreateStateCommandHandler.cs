using UserManagement.Domain.Entities;
using AutoMapper;
using MediatR;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Domain.Events;
using FluentValidation;

namespace UserManagement.Application.State.Commands.CreateState
{
    public class CreateStateCommandHandler : IRequestHandler<CreateStateCommand, StateDto>     
    {
        private readonly IMapper _mapper;
        private readonly IStateCommandRepository _stateRepository;
        private readonly IMediator _mediator; 

        // Constructor Injection
        public CreateStateCommandHandler(IMapper mapper, IStateCommandRepository stateRepository, IMediator mediator)
        {
            _mapper = mapper;
            _stateRepository = stateRepository;
            _mediator = mediator;
        }

        public async Task<StateDto> Handle(CreateStateCommand request, CancellationToken cancellationToken)
        {        
            var countryExists = await _stateRepository.CountryExistsAsync(request.CountryId);
            if (!countryExists)
            {
                throw new ValidationException("Invalid CountryId. The specified country does not exist or is inactive.");
                                 
            }            
            var stateExists = await _stateRepository.GetStateByCodeAsync(request.StateName ?? string.Empty, 
                request.StateCode ?? string.Empty,      request.CountryId ) ;
            if (stateExists.Id !=0)
            {
                throw new ValidationException("StateName & Code already exists in the specified country.");
               
            }            
            var stateEntity = _mapper.Map<States>(request);
            var result = await _stateRepository.CreateAsync(stateEntity);
            //Domain Event
            if (result != null)
            {       
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: result.StateCode ?? string.Empty,
                    actionName: result.StateName ?? string.Empty,
                    details: $"State '{result.StateName}' was created. StateCode: {result.StateCode}",
                    module:"State"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            
            
            var stateDto = _mapper.Map<StateDto>(result);
            if (stateDto.Id > 0)
            {
                return stateDto;
            }
            throw new Exception("State not created.");
                   
        }
      
    }
}