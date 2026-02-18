using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter
{
    public class GetWorkCenterQueryHandler : IRequestHandler<GetWorkCenterQuery, ApiResponseDTO<List<WorkCenterDto>>>
    {
        private readonly IWorkCenterQueryRepository _iWorkCenterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;

        public GetWorkCenterQueryHandler(
            IWorkCenterQueryRepository iWorkCenterQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup)
        {
            _iWorkCenterQueryRepository = iWorkCenterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }

        public async Task<ApiResponseDTO<List<WorkCenterDto>>> Handle(GetWorkCenterQuery request, CancellationToken cancellationToken)
        {
            var (WorkCenter, totalCount) = await _iWorkCenterQueryRepository.GetAllWorkCenterGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var workCentersgrouplist = _mapper.Map<List<WorkCenterDto>>(WorkCenter);
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            // 🔥 Map department & unit names with DataControl to costCenters
            foreach (var dto in workCentersgrouplist)
            {
                if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.DepartmentName = deptName;

                if (unitLookup.TryGetValue(dto.UnitId, out var unitName))
                    dto.UnitName = unitName;
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetWorkCenter",
                actionCode: "Get",
                actionName: WorkCenter.Count().ToString(),
                details: $"WorkCenter details was fetched.",
                module: "WorkCenter"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<WorkCenterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = workCentersgrouplist,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

    }
}
