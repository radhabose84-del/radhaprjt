using AutoMapper;
// using Contracts.Interfaces.External.IUser;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.Location.Queries.GetLocations
{
    public class GetLocationHandlerQuery : IRequestHandler<GetLocationQuery, ApiResponseDTO<List<LocationDto>>>
    {
        private readonly ILocationQueryRepository _locationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        // private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;

        public GetLocationHandlerQuery(ILocationQueryRepository locationQueryRepository, IMediator mediator, IMapper mapper
        // , IDepartmentAllGrpcClient departmentService
        )
        {
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            // _departmentAllGrpcClient = departmentService;
        }
        public async Task<ApiResponseDTO<List<LocationDto>>> Handle(GetLocationQuery request, CancellationToken cancellationToken)
        {
            var (locations, totalCount) = await _locationQueryRepository.GetAllLocationAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var locationList = _mapper.Map<List<LocationDto>>(locations);

            // 🔥 Fetch departments using gRPC
            // var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();

            // var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            // var LocationDictionary = new Dictionary<int, LocationDto>();

            // 🔥 Map department names with DataControl to location
            // foreach (var data in locationList)
            // {

            //     if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
            //     {
            //         data.DepartmentName = departmentName;
            //     }

            //     LocationDictionary[data.DepartmentId] = data;

            // }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLocations",
                actionCode: "",
                actionName: "",
                details: $"Location details was fetched.",
                module: "Location"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<LocationDto>>
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