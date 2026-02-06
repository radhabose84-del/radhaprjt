using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster
{
    public class GetAllActivityMasterQueryHandler : IRequestHandler<GetAllActivityMasterQuery, ApiResponseDTO<List<GetAllActivityMasterDto>>>
    {
        private readonly IActivityMasterQueryRepository _activityMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;

        public GetAllActivityMasterQueryHandler(
            IActivityMasterQueryRepository activityMasterQueryRepository,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup)
        {
            _activityMasterQueryRepository = activityMasterQueryRepository;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }

   
        
        public async Task<ApiResponseDTO<List<GetAllActivityMasterDto>>> Handle( GetAllActivityMasterQuery request,  CancellationToken cancellationToken)
        {
            var (activities, totalCount) =
                await _activityMasterQueryRepository.GetAllActivityMasterAsync(
                    request.PageNumber, request.PageSize, request.SearchTerm);

            // If repo already returns DTO, mapping is unnecessary
            var activityList = activities.ToList();

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments
                .GroupBy(d => d.DepartmentId)
                .ToDictionary(g => g.Key, g => g.First().DepartmentName);

                    // ✅ FILTER HERE (by department master existence)
            activityList = activityList
                .Where(a => departmentLookup.ContainsKey(a.DepartmentId))
                .ToList();

            var units = await _unitLookup.GetAllUnitAsync();
            var unitLookup = units
                .GroupBy(u => u.UnitId)
                .ToDictionary(g => g.Key, g => g.First().UnitName);

            foreach (var data in activityList)
            {
                data.DepartmentName = departmentLookup.TryGetValue(data.DepartmentId, out var deptName)
                    ? deptName
                    : null; // or "Unknown"

                data.UnitName = unitLookup.TryGetValue(data.UnitId, out var unitName)
                    ? unitName
                    : null;
            }

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Activity Master details were fetched.",
                module: "ActivityMaster"
            ), cancellationToken);

            return new ApiResponseDTO<List<GetAllActivityMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = activityList,          // ✅ return all paged items
                TotalCount = activityList.Count,      // ✅ DB total count
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
