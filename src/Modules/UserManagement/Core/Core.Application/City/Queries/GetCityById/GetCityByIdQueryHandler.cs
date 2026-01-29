using AutoMapper;
using Core.Application.City.Queries.GetCities;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.ICity;
using Core.Domain.Events;
using FluentValidation;
using MediatR;

namespace Core.Application.City.Queries.GetCityById
{
    public class GetCityByIdQueryHandler : IRequestHandler<GetCityByIdQuery,CityDto>
    {
        private readonly ICityQueryRepository _cityRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCityByIdQueryHandler(ICityQueryRepository cityRepository, IMapper mapper, IMediator mediator)
        {
           _cityRepository = cityRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<CityDto> Handle(GetCityByIdQuery request, CancellationToken cancellationToken)
        {                    
            var city = await _cityRepository.GetByIdAsync(request.Id);                
            var cityDto = _mapper.Map<CityDto>(city);
            if (city is null)
            {        
                throw new ValidationException("City with ID {request.Id} not found.");        
               
            }       
                //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: cityDto.CityCode ?? string.Empty,        
                actionName: cityDto.CityName ?? string.Empty,                
                details: $"City '{cityDto.CityName}' was created. CityCode: {cityDto.CityCode}",
                module:"City"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  cityDto;              
        }
    }
}