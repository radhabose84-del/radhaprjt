#nullable disable
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

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetCheckListByActivityId
{
  
    public class GetActivityCheckListByActivityIdQueryHandler : IRequestHandler<GetActivityCheckListByActivityIdQuery, List<GetActivityCheckListByActivityIdDto>>
    {
        private readonly IActivityCheckListMasterQueryRepository _activityCheckListMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;

        public GetActivityCheckListByActivityIdQueryHandler(
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

        public async Task<List<GetActivityCheckListByActivityIdDto>> Handle(GetActivityCheckListByActivityIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _activityCheckListMasterQueryRepository.GetCheckListByActivityIdsAsync(request.Ids);

            // if (result == null || !result.Any())
            // {
            //     return new ApiResponseDTO<List<GetActivityCheckListByActivityIdDto>>
            //     {
            //         IsSuccess = false,
            //         Message = $"No activity checklists found for ActivityIds: {string.Join(", ", request.Ids)}.",
            //         Data = null
            //     };
            // }

            var checklistDtos = _mapper.Map<List<GetActivityCheckListByActivityIdDto>>(result);
             
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);


            var activityMasterDictionary = new Dictionary<int, GetActivityCheckListByActivityIdDto>();

            var units = await _unitLookup.GetAllUnitAsync();
            var unitslookup = units.ToDictionary(d => d.UnitId, d => d.UnitName);  
             foreach (var data in checklistDtos)
            {

                if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
                {

                    data.DepartmentName = departmentName;
                }
              
                if (unitslookup.TryGetValue(data.UnitId, out var unitName) && unitName != null)
                {
                    data.UnitName = unitName;
                }
             
               
            } 
            
               
               var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByActivityId",
                actionCode: string.Join(", ", request.Ids),  // Ensure activity IDs are joined as a string
                actionName: "ActivityCheckList",
                details: $"Fetched {checklistDtos.Count} checklist(s) for ActivityIds: {string.Join(", ", request.Ids)}.", // Better message formatting
                module: "ActivityCheckListMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return checklistDtos;
        }
    }
}
