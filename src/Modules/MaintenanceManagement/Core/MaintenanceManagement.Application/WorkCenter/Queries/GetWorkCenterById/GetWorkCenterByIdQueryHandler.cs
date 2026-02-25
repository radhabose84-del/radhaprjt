using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterById
{
    public class GetWorkCenterByIdQueryHandler : IRequestHandler<GetWorkCenterByIdQuery, ApiResponseDTO<WorkCenterDto>>
    {
        private readonly IWorkCenterQueryRepository _iWorkCenterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;

        public GetWorkCenterByIdQueryHandler(
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

        public async Task<ApiResponseDTO<WorkCenterDto>> Handle(GetWorkCenterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iWorkCenterQueryRepository.GetByIdAsync(request.Id);
            if (result is null)
                return new ApiResponseDTO<WorkCenterDto> { IsSuccess = false, Message = $"WorkCenter ID {request.Id} not found." };

            var workCenter = _mapper.Map<WorkCenterDto>(result);
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var units = await _unitLookup.GetAllUnitAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            if ((departmentLookup.TryGetValue(workCenter.DepartmentId, out var departmentName) && departmentName != null) |
                 (unitLookup.TryGetValue(workCenter.UnitId, out var unitName) && unitName != null))
            {
                workCenter.DepartmentName = departmentName;
                workCenter.UnitName = unitName;
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetWorkCenterByIdQuery",
                actionName: workCenter.Id.ToString(),
                details: $"WorkCenter details {workCenter.Id} was fetched.",
                module: "WorkCenter");
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<WorkCenterDto> { IsSuccess = true, Message = "Success", Data = workCenter };
        }
    }
}
