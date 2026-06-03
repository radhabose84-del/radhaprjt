using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Location.Queries.GetAllLocation
{
    public class GetAllLocationQueryHandler : IRequestHandler<GetAllLocationQuery, ApiResponseDTO<List<GetAllLocationDto>>>
    {
        private readonly ILocationQueryRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllLocationQueryHandler(ILocationQueryRepository locationRepository, IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _locationRepository = locationRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GetAllLocationDto>>> Handle(GetAllLocationQuery request, CancellationToken cancellationToken)
        {
            var (locations, totalCount) = await _locationRepository
                .GetAllLocationAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            if (locations == null || !locations.Any())
            {
                return new ApiResponseDTO<List<GetAllLocationDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found"
                };
            }

            var locationList = _mapper.Map<List<GetAllLocationDto>>(locations);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Location details were fetched.",
                module: "Location");

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GetAllLocationDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = locationList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
