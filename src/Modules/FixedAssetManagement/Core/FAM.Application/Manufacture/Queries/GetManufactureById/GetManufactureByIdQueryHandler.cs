using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.Manufacture.Queries.GetManufactureById
{
    public class GetManufactureByIdQueryHandler : IRequestHandler<GetManufactureByIdQuery, ManufactureDTO>
    {
        private readonly IManufactureQueryRepository _manufactureRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        // private readonly ILocationLookupService _locationLookupService;  

        public GetManufactureByIdQueryHandler(IManufactureQueryRepository manufactureRepository, IMapper mapper, IMediator mediator
        // , ILocationLookupService locationLookupService
        )
        {
            _manufactureRepository =manufactureRepository;
            _mapper =mapper;
            _mediator = mediator;
            // _locationLookupService=locationLookupService;
        }
        public async Task<ManufactureDTO> Handle(GetManufactureByIdQuery request, CancellationToken cancellationToken)
        {
            var manufacture = await _manufactureRepository.GetByIdAsync(request.Id);                            
            if (manufacture is null)
            {           
                throw new ValidationException("Manufacture with ID {request.Id} not found.");     
               
            }       
            var manufactureDto = _mapper.Map<ManufactureDTO>(manufacture);
            // Get lookup data for geo enrichment
            // var countries = await _locationLookupService.GetCountryLookupAsync();
            // var states = await _locationLookupService.GetStateLookupAsync();
            // var cities = await _locationLookupService.GetCityLookupAsync();

            // if (countries.TryGetValue(manufactureDto.CountryId, out var countryName))
            //     manufactureDto.CountryName = countryName;

            // if (states.TryGetValue(manufactureDto.StateId, out var stateName))
            //     manufactureDto.StateName = stateName;

            // if (cities.TryGetValue(manufactureDto.CityId, out var cityName))
            //     manufactureDto.CityName = cityName;
                
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: manufactureDto.Code ?? string.Empty,        
                actionName: manufactureDto.ManufactureName ?? string.Empty,                
                details: $"Manufacture '{manufactureDto.ManufactureName}' was created. Code: {manufactureDto.Code}",
                module:"Manufacture"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  manufactureDto;       
        }
    }
}