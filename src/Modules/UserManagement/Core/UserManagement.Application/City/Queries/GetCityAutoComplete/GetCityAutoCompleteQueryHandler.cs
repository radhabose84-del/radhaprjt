using AutoMapper;
using MediatR;
using UserManagement.Application.City.Queries.GetCities; 
using UserManagement.Application.Common.Interfaces.ICity;
using UserManagement.Domain.Events;
using UserManagement.Application.Common.HttpResponse;
using FluentValidation;

namespace UserManagement.Application.City.Queries.GetCityAutoComplete
{
    public class GetCityAutoCompleteQueryHandler : IRequestHandler<GetCityAutoCompleteQuery, List<CityAutoCompleteDTO>>
    {
        private readonly ICityQueryRepository _cityRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetCityAutoCompleteQueryHandler(ICityQueryRepository cityRepository,  IMapper mapper, IMediator mediator)
        {
            _cityRepository =cityRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<List<CityAutoCompleteDTO>> Handle(GetCityAutoCompleteQuery request, CancellationToken cancellationToken)
        {             
            var result = await _cityRepository.GetByCityNameAsync(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No Cities found matching the search pattern.");
               
            }
            var cityDto = _mapper.Map<List<CityAutoCompleteDTO>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"City '{request.SearchPattern}' was searched",
                module:"City"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  cityDto;          
        }
    }  
}