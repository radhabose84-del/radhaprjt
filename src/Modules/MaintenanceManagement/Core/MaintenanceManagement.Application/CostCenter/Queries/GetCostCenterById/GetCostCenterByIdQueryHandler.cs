using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterById
{
    public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, CostCenterDto?>
    {

        private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;


        public GetCostCenterByIdQueryHandler(
            ICostCenterQueryRepository iCostCenterQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup)
        {
            _iCostCenterQueryRepository = iCostCenterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }

        public async Task<CostCenterDto?> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iCostCenterQueryRepository.GetByIdAsync(request.Id);

            // Not found (or out of the caller's unit scope) → return null so the controller maps it to 404
            // instead of dereferencing a null DTO (was throwing NullReferenceException → 500).
            if (result == null)
                return null;

            var costCenter = _mapper.Map<CostCenterDto>(result);


            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var units = await _unitLookup.GetAllUnitAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            if ((departmentLookup.TryGetValue(costCenter.DepartmentId, out var departmentName) && departmentName != null) |
                 (unitLookup.TryGetValue(costCenter.UnitId, out var unitName) && unitName != null))
            {
                costCenter.DepartmentName = departmentName;
                costCenter.UnitName = unitName;
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCostCenterByIdQuery",
                actionName: costCenter.Id.ToString(),
                details: $"CostCenter details {costCenter.Id} was fetched.",
                module: "CostCenter"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return costCenter;
        }

    }
}
