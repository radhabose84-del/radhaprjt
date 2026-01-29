using AutoMapper;
// using Contracts.Interfaces.External.IUser;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.SubLocation.Queries.GetSubLocations
{
    public class GetSubLocationHandlerQuery : IRequestHandler<GetSubLocationQuery, ApiResponseDTO<List<SubLocationDto>>>
    {
        private readonly ISubLocationQueryRepository _sublocationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        // private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;


        public GetSubLocationHandlerQuery(ISubLocationQueryRepository sublocationQueryRepository, IMapper mapper, IMediator mediator
        // , IDepartmentAllGrpcClient departmentAllGrpcClient
        )
        {
            _sublocationQueryRepository = sublocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            // _departmentAllGrpcClient = departmentAllGrpcClient;

        }
        public async Task<ApiResponseDTO<List<SubLocationDto>>> Handle(GetSubLocationQuery request, CancellationToken cancellationToken)
        {
            var (sublocations, totalCount) = await _sublocationQueryRepository.GetAllSubLocationAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var sublocationList = _mapper.Map<List<SubLocationDto>>(sublocations);

            // 🔥 Fetch departments using gRPC
            // var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
            // var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            // var subLocationDictionary = new Dictionary<int, SubLocationDto>();

            // 🔥 Map department names with DataControl to location
            // foreach (var data in sublocationList)
            // {

            //     if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
            //     {
            //         data.DepartmentName = departmentName;
            //     }

            //     subLocationDictionary[data.DepartmentId] = data;

            // }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetSubLocations",
                actionCode: "",
                actionName: "",
                details: $"SubLocation details was fetched.",
                module: "SubLocation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<SubLocationDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = sublocationList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}