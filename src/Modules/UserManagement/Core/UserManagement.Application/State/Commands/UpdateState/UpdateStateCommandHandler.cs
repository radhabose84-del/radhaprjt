using MediatR;
using UserManagement.Domain.Entities;
using AutoMapper;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Domain.Events;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Domain.Enums.Common;
using FluentValidation;

namespace UserManagement.Application.State.Commands.UpdateState
{    
    public class UpdateStateCommandHandler : IRequestHandler<UpdateStateCommand, bool>
    {
        private readonly IStateCommandRepository _stateRepository;
        private readonly IMapper _mapper;
        private readonly IStateQueryRepository _stateQueryRepository;
        private readonly IMediator _mediator; 

        public UpdateStateCommandHandler(IStateCommandRepository stateRepository, IMapper mapper, IStateQueryRepository stateQueryRepository, IMediator mediator)
        {
            _stateRepository = stateRepository;
            _mapper = mapper;
            _stateQueryRepository = stateQueryRepository;
            _mediator = mediator;
        }        
        public async Task<bool> Handle(UpdateStateCommand request, CancellationToken cancellationToken)
        {
            var state = await _stateQueryRepository.GetByIdAsync(request.Id);
             if (state is null)        
             throw new ValidationException("State not found");
             
            if (request.IsActive == 0) // Inactive
            {
                var linked = await _stateQueryRepository.IsLinkedWithCitiesAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

            var oldStateName = state.StateName;
            state.StateName = request.StateName;
            if (state is null || state.IsDeleted is Enums.IsDelete.Deleted)
            {
                throw new ValidationException("Invalid StateID. The specified State does not exist or is inactive.");     
              
            }
            var countryExists = await _stateRepository.CountryExistsAsync(request.CountryId);
            if (!countryExists)
            {       
                 throw new ValidationException("Invalid CountryID. The specified Country does not exist or is inactive.");        
              
            }
            if ((byte)state.IsActive != request.IsActive)
            {    
                 state.IsActive =  (Enums.Status)request.IsActive;             
                await _stateRepository.UpdateAsync(state.Id, state);
                if (request.IsActive is 0)
                {
                    
                    return true;
                }
                else{
                    return true; 
                }                                     
            }
            var stateExists = await _stateRepository.GetStateByCodeAsync(request.StateName ?? string.Empty,request.StateCode ??string.Empty, request.CountryId);            
            if (stateExists.Id !=0)
            {              
                if ((byte)stateExists.IsActive == request.IsActive)
                {             
                    throw new ValidationException($"StateCode already exists and is {(Enums.Status)request.IsActive}.");       
                                                
                }               
            }
            var updatedStateEntity = _mapper.Map<States>(request);          
            var updateResult = await _stateRepository.UpdateAsync(request.Id, updatedStateEntity);            
            
            var updatedState = await _stateQueryRepository.GetByIdAsync(request.Id);              
            if (updatedState != null)
            {                    
                var stateDto = _mapper.Map<StateDto>(updatedState);

                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: stateDto.StateCode ?? string.Empty,
                    actionName: stateDto.StateName ?? string.Empty,                           
                    details: $"State '{oldStateName}' was updated to '{stateDto.StateName}'.  StateCode: {stateDto.StateCode}",
                    module:"State"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                if(updateResult>0)
                {
                    return true;
                }
                throw new Exception("State not updated.");
              
            }
            
            else
            {
                throw new ValidationException("State not found.");
             
            }                
        }    
    }
}