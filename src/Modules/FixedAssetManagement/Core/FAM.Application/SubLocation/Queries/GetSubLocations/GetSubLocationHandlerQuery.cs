using AutoMapper;
using Contracts.Interfaces.Lookups.Users;

// using Contracts.Interfaces.External.IUser;
using Contracts.Common;
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
        private readonly IDepartmentLookup _departmentLookup; 


        public GetSubLocationHandlerQuery(ISubLocationQueryRepository sublocationQueryRepository, IMapper mapper, IMediator mediator
        , IDepartmentLookup departmentLookup
        )
        {
            _sublocationQueryRepository = sublocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;

        }
        public async Task<ApiResponseDTO<List<SubLocationDto>>> Handle(GetSubLocationQuery request, CancellationToken cancellationToken)
        {
            var (sublocations, totalCount) = await _sublocationQueryRepository.GetAllSubLocationAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var sublocationList = _mapper.Map<List<SubLocationDto>>(sublocations);

            // ✅ Enrich DepartmentName using lookup interface (UserManagement owner)
            var deptIds = sublocationList
                .Select(x => x.DepartmentId)
                .Where(x => x > 0)
                .Distinct()
                .ToArray();

            if (deptIds.Length > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(deptIds, cancellationToken);

                // DepartmentLookupDto: DepartmentId, DeptName
                var deptMap = departments
                    .Where(d => d != null)
                    .ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

                foreach (var item in sublocationList)
                {
                    if (deptMap.TryGetValue(item.DepartmentId, out var deptName))
                    {
                        item.DepartmentName = deptName;
                    }
                }
            }

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