using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster
{
    public class GetAllActivityCheckListMasterQueryHandler : IRequestHandler<GetAllActivityCheckListMasterQuery, ApiResponseDTO<List<GetAllActivityCheckListMasterDto>>>
    {
        private readonly IActivityCheckListMasterQueryRepository _activityCheckListMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;

        public GetAllActivityCheckListMasterQueryHandler(
            IActivityCheckListMasterQueryRepository activityCheckListMasterQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup)
        {
            _activityCheckListMasterQueryRepository = activityCheckListMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }

        public async Task<ApiResponseDTO<List<GetAllActivityCheckListMasterDto>>> Handle(GetAllActivityCheckListMasterQuery request, CancellationToken cancellationToken)
        {
            var (checkLists, totalCount) = await _activityCheckListMasterQueryRepository.GetAllActivityCheckListMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var checkListDto = _mapper.Map<List<GetAllActivityCheckListMasterDto>>(checkLists);

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var units = await _unitLookup.GetAllUnitAsync();
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var data in checkListDto)
            {
                if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
                {
                    data.DepartmentName = departmentName;
                }

                if (unitLookup.TryGetValue(data.UnitId, out var unitName) && unitName != null)
                {
                    data.UnitName = unitName;
                }
            }

            var filteredActivities = checkListDto
                .Where(p => departmentLookup.ContainsKey(p.DepartmentId))
                .ToList();

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Activity Checklist Master details were fetched.",
                module: "ActivityCheckListMaster");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GetAllActivityCheckListMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = filteredActivities,
                TotalCount = filteredActivities.Count,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
