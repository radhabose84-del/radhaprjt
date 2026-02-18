using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common;
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Events;
using Contracts.Common;
using FluentValidation;

namespace UserManagement.Application.City.Commands.CreateCity
{    
    public class CreateCityCommandHandler : IRequestHandler<CreateCityCommand, CityDto>
    {
        private readonly IMapper _mapper;
        private readonly ICityCommandRepository _cityRepository;
        private readonly IMediator _mediator; 

        // Constructor Injection
        public CreateCityCommandHandler(IMapper mapper, ICityCommandRepository cityRepository, IMediator mediator)
        {
            _mapper = mapper;
            _cityRepository = cityRepository;
            _mediator = mediator;    
        }

        public async Task<CityDto> Handle(CreateCityCommand request, CancellationToken cancellationToken)
        {
            var stateExists = await _cityRepository.StateExistsAsync(request.StateId);
            if (!stateExists)
            {
                throw new ValidationException("Invalid StateId. The specified state does not exist or is inactive.");
                             
            }      
            // Check if the city name already exists in the same state
            var cityExistsByName = await _cityRepository.GetCityByNameAsync(request.CityName ?? string.Empty,request.CityCode ?? string.Empty, request.StateId) ;            
            if (cityExistsByName.Id !=0)
            {
                throw new ValidationException("City name & code already exists in the specified state.");
              
            }    
            var cityEntity = _mapper.Map<Cities>(request);            
            var result = await _cityRepository.CreateAsync(cityEntity);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: result.CityCode ?? string.Empty,
                actionName: result.CityName ?? string.Empty,
                details: $"City '{result.CityName}' was created. CityCode: {result.CityCode}",
                module:"City"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
            var cityDto = _mapper.Map<CityDto>(result);
            if (cityDto.Id > 0)
            {
                return cityDto;
            }
            throw new Exception("City not created.");
                      
        }
    }
}
