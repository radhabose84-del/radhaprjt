using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces;
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
        // private readonly ILocationLookupService _locationLookupService;  

        public GetManufactureQueryHandler(IManufactureQueryRepository manufactureRepository, IMapper mapper, IMediator mediator
        // , ILocationLookupService locationLookupService
        )
        {
            _manufactureRepository = manufactureRepository;
            _mapper = mapper;
            _mediator = mediator;
            // _locationLookupService=locationLookupService;
        }
        public async Task<ApiResponseDTO<List<ManufactureDTO>>> Handle(GetManufactureQuery request, CancellationToken cancellationToken)
        {
            var (manufacture, totalCount) = await _manufactureRepository.GetAllManufactureAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var manufactureList = _mapper.Map<List<ManufactureDTO>>(manufacture);
           
            // Get lookup data for geo enrichment
            // var countries = await _locationLookupService.GetCountryLookupAsync();
            // var states = await _locationLookupService.GetStateLookupAsync();
            // var cities = await _locationLookupService.GetCityLookupAsync();

            // Enrich DTOs with country/state/city names
            // foreach (var m in manufactureList)
            // {
            //     if (countries.TryGetValue(m.CountryId, out var countryName))
            //         m.CountryName = countryName;

            //     if (states.TryGetValue(m.StateId, out var stateName))
            //         m.StateName = stateName;

            //     if (cities.TryGetValue(m.CityId, out var cityName))
            //         m.CityName = cityName;
            // }

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