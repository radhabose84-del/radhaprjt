using MediatR;
using Core.Domain.Entities;
using AutoMapper;
using Core.Application.City.Queries.GetCities;
using Core.Application.Common.Interfaces.ICity;
using Core.Domain.Events;
using Core.Application.Common.HttpResponse;
using Core.Domain.Enums.Common;
using FluentValidation;

namespace Core.Application.City.Commands.UpdateCity
{       
    public class UpdateCityCommandHandler : IRequestHandler<UpdateCityCommand, CityDto>
    {
        private readonly ICityCommandRepository _cityRepository;
        private readonly ICityQueryRepository _cityQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public UpdateCityCommandHandler(ICityCommandRepository cityRepository, IMapper mapper,ICityQueryRepository cityQueryRepository, IMediator mediator)
        {
            _cityRepository = cityRepository;
            _mapper = mapper;
            _cityQueryRepository = cityQueryRepository;
            _mediator = mediator;
        }
        public async Task<CityDto> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
        {
            var city = await _cityQueryRepository.GetByIdAsync(request.Id);
            if (city is null)
            throw new ValidationException("Invalid CityID. The specified City does not exist or is inactive.");

            if (city.IsDeleted is Enums.IsDelete.Deleted)
                throw new ValidationException("Invalid CityID. The specified City does not exist or is deleted.");

            if (request.IsActive == 0) // Inactive
            {
                var linked = await _cityQueryRepository.IsCityLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

            // ✅ Validate State (keep existing behavior)
            var stateExists = await _cityRepository.StateExistsAsync(request.StateId);
            if (!stateExists)
                throw new ValidationException("Invalid StateId. The specified state does not exist or is inactive.");

            // ✅ STATUS-ONLY UPDATE: update and RETURN (no duplicate validations)
            if ((byte)city.IsActive != request.IsActive)
            {
                city.IsActive = (Enums.Status)request.IsActive;
                await _cityRepository.UpdateAsync(city.Id, city);

                var updatedAfterStatus = await _cityQueryRepository.GetByIdAsync(request.Id);
                if (updatedAfterStatus is null)
                    throw new ValidationException("City not found.");

                return _mapper.Map<CityDto>(updatedAfterStatus);
            }

            // NOTE: Prevent false duplicate when it's the SAME record.
            var cityExistsByName = await _cityRepository.GetCityByNameAsync(
                request.CityName ?? string.Empty,
                request.CityCode ?? string.Empty,
                request.StateId
            );

            if (cityExistsByName != null && cityExistsByName.Id != 0 && cityExistsByName.Id != request.Id)
            {
                if ((byte)cityExistsByName.IsActive == request.IsActive)
                    throw new ValidationException($"City Code already exists and is {(Enums.Status)request.IsActive}.");
            }

            var oldCityName = city.CityName;

            var updatedCityEntity = _mapper.Map<Cities>(request);
            var updateResult = await _cityRepository.UpdateAsync(request.Id, updatedCityEntity);

            var updatedCity =  await _cityQueryRepository.GetByIdAsync(request.Id);    
            if (updatedCity != null)
            {
                var cityDto = _mapper.Map<CityDto>(updatedCity);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: cityDto.CityCode ?? string.Empty,
                    actionName: cityDto.CityName ?? string.Empty,
                    details: $"City '{oldCityName}' was updated to '{cityDto.CityName}'. CityCode: {cityDto.CityCode}",
                    module: "City"
                );

                await _mediator.Publish(domainEvent, cancellationToken);
                if(updateResult>0)
                {
                    return  cityDto;
                }
                throw new Exception("City not updated.");
                        
            }
            else
            {
                throw new ValidationException("City not found.");
             
            }
            
        }
    }
}