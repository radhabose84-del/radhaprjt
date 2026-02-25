using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using Contracts.Common;
using FAM.Application.Common.Interfaces.IManufacture;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.Manufacture.Queries.GetManufacture
{
    public class GetManufactureQueryHandler : IRequestHandler<GetManufactureQuery, ApiResponseDTO<List<ManufactureDTO>>>
    {
        private readonly IManufactureQueryRepository _manufactureRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICountryLookup _countryLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICityLookup _cityLookup;

        public GetManufactureQueryHandler(IManufactureQueryRepository manufactureRepository, IMapper mapper, IMediator mediator,
            ICountryLookup countryLookup, IStateLookup stateLookup, ICityLookup cityLookup)
        {
            _manufactureRepository = manufactureRepository;
            _mapper = mapper;
            _mediator = mediator;
            _countryLookup = countryLookup;
            _stateLookup = stateLookup;
            _cityLookup = cityLookup;
        }
        public async Task<ApiResponseDTO<List<ManufactureDTO>>> Handle(GetManufactureQuery request, CancellationToken cancellationToken)
        {
            var (manufacture, totalCount) = await _manufactureRepository.GetAllManufactureAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var manufactureList = _mapper.Map<List<ManufactureDTO>>(manufacture);

            // Enrich Country names via lookup
            var countryIds = manufactureList
                .Select(x => x.CountryId)
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (countryIds.Length > 0)
            {
                var countries = await _countryLookup.GetByIdsAsync(countryIds, cancellationToken);
                var countryMap = countries.Where(c => c != null).ToDictionary(c => c.CountryId, c => c.CountryName);

                foreach (var m in manufactureList)
                {
                    if (countryMap.TryGetValue(m.CountryId, out var countryName))
                        m.CountryName = countryName;
                }
            }

            // Enrich State names via lookup
            var stateIds = manufactureList
                .Select(x => x.StateId)
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (stateIds.Length > 0)
            {
                var states = await _stateLookup.GetByIdsAsync(stateIds, cancellationToken);
                var stateMap = states.Where(s => s != null).ToDictionary(s => s.StateId, s => s.StateName);

                foreach (var m in manufactureList)
                {
                    if (stateMap.TryGetValue(m.StateId, out var stateName))
                        m.StateName = stateName;
                }
            }

            // Enrich City names via lookup
            var cityIds = manufactureList
                .Select(x => x.CityId)
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (cityIds.Length > 0)
            {
                var cities = await _cityLookup.GetByIdsAsync(cityIds, cancellationToken);
                var cityMap = cities.Where(c => c != null).ToDictionary(c => c.CityId, c => c.CityName);

                foreach (var m in manufactureList)
                {
                    if (cityMap.TryGetValue(m.CityId, out var cityName))
                        m.CityName = cityName;
                }
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Manufacture details was fetched.",
                module:"Manufacture"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<ManufactureDTO>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = manufactureList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };            
        }
    }
}