using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
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
        private readonly ICountryLookup _countryLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICityLookup _cityLookup;
        // private readonly ILocationLookupService _locationLookupService;  

        public GetManufactureByIdQueryHandler(IManufactureQueryRepository manufactureRepository, IMapper mapper, IMediator mediator,
            ICountryLookup countryLookup, IStateLookup stateLookup, ICityLookup cityLookup
        // , ILocationLookupService locationLookupService
        )
        {
            _manufactureRepository =manufactureRepository;
            _mapper =mapper;
            _mediator = mediator;
            _countryLookup = countryLookup;
            _stateLookup = stateLookup;
            _cityLookup = cityLookup;
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

            if (manufactureDto.CountryId > 0)
            {
                var country = await _countryLookup.GetByIdAsync(manufactureDto.CountryId, cancellationToken);
                if (country != null)
                    manufactureDto.CountryName = country.CountryName;
            }

            if (manufactureDto.StateId > 0)
            {
                var state = await _stateLookup.GetByIdAsync(manufactureDto.StateId, cancellationToken);
                if (state != null)
                    manufactureDto.StateName = state.StateName;
            }

            if (manufactureDto.CityId > 0)
            {
                var city = await _cityLookup.GetByIdAsync(manufactureDto.CityId, cancellationToken);
                if (city != null)
                    manufactureDto.CityName = city.CityName;
            }
                
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
