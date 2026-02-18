using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users; // ✅ lookup contract
using Contracts.Common;
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
        private readonly IDepartmentLookup _departmentLookup;  // ✅ lookup dependency

        public GetLocationHandlerQuery(ILocationQueryRepository locationQueryRepository, IMediator mediator, IMapper mapper,
            IDepartmentLookup departmentLookup) // ✅ inject lookup
        {
            _locationQueryRepository = locationQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _departmentLookup = departmentLookup;
        }
        public async Task<ApiResponseDTO<List<LocationDto>>> Handle(GetLocationQuery request, CancellationToken cancellationToken)
        {
            var (list, totalCount) =
                await _locationQueryRepository.GetAllLocationListAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.SearchTerm);

            // ✅ Enrich DepartmentName using lookup interface (UserManagement owner)
            var deptIds = list
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

                foreach (var item in list)
                {
                    if (deptMap.TryGetValue(item.DepartmentId, out var deptName))
                    {
                        item.DepartmentName = deptName;
                    }
                }
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLocations",
                actionCode: "Get",
                actionName: list.Count.ToString(),
                details: "Location details was fetched.",
                module: "Location"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<LocationDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = list,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
