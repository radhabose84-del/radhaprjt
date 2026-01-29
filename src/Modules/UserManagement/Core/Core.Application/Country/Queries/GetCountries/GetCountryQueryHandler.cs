using MediatR;
using AutoMapper;
using Core.Application.Common.Interfaces.ICountry;
using Core.Domain.Events;
using Core.Application.Common.HttpResponse;

namespace Core.Application.Country.Queries.GetCountries
{
    public class GetCountryQueryHandler : IRequestHandler<GetCountryQuery, ApiResponseDTO<List<CountryDto>>>
    {
        private readonly ICountryQueryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        public GetCountryQueryHandler(ICountryQueryRepository countryRepository , IMapper mapper, IMediator mediator)
        {
            _countryRepository = countryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<CountryDto>>> Handle(GetCountryQuery request, CancellationToken cancellationToken)
        {            
            var (countries, totalCount)= await _countryRepository.GetAllCountriesAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var countriesList = _mapper.Map<List<CountryDto>>(countries);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Country details was fetched.",
                module:"Country"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<CountryDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = countriesList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
           
        }
    }
}