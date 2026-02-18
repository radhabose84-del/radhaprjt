using UserManagement.Application.Country.Queries.GetCountries;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICountry;
using UserManagement.Domain.Events;
using Contracts.Common;
using FluentValidation;

namespace UserManagement.Application.Country.Queries.GetCountryAutoComplete
{
    public class GetCountryAutoCompleteQueryHandler : IRequestHandler<GetCountryAutoCompleteQuery, List<CountryAutoCompleteDTO>>
    {
        private readonly ICountryQueryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetCountryAutoCompleteQueryHandler(ICountryQueryRepository countryRepository, IMapper mapper, IMediator mediator)
        {
            _countryRepository =countryRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<List<CountryAutoCompleteDTO>> Handle(GetCountryAutoCompleteQuery request, CancellationToken cancellationToken)
        {   
                   
            var result = await _countryRepository.GetByCountryNameAsync(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No countries found matching the search pattern.");
             
            }
            var countryDto = _mapper.Map<List<CountryAutoCompleteDTO>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"Country '{request.SearchPattern}' was searched",
                module:"Country"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return countryDto;                        
        }
    }
  
}