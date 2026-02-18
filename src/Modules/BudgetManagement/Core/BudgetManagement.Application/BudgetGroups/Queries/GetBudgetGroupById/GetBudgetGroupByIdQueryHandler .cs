using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using BudgetManagement.Application.BudgetGroups;
using Contracts.Common;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupById
{
    public class GetBudgetGroupByIdQueryHandler
        : IRequestHandler<GetBudgetGroupByIdQuery, ApiResponseDTO<BudgetGroupDto>>
    {
        private readonly IBudgetGroupQueryRepository _budgetGroupQueryRepository;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ICostCenterLookup _costCenterLookup;

        public GetBudgetGroupByIdQueryHandler(
            IBudgetGroupQueryRepository budgetGroupQueryRepository,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup,
            ICostCenterLookup costCenterLookup)
        {
            _budgetGroupQueryRepository = budgetGroupQueryRepository;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
            _costCenterLookup = costCenterLookup;
        }

        public async Task<ApiResponseDTO<BudgetGroupDto>> Handle(
            GetBudgetGroupByIdQuery request,
            CancellationToken cancellationToken)
        {
            var dto = await _budgetGroupQueryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (dto == null)
                throw new ExceptionRules("Budget Group not found.");

            // Department enrichment
            if (string.IsNullOrWhiteSpace(dto.DepartmentName) && dto.DepartmentId > 0)
            {
                var dept = await _departmentLookup.GetByIdAsync(dto.DepartmentId, cancellationToken);
                if (dept != null)
                    dto.DepartmentName = dept.DepartmentName;
            }

            // Unit enrichment
            if (string.IsNullOrWhiteSpace(dto.UnitName) && dto.UnitId > 0)
            {
                var unit = await _unitLookup.GetByIdAsync(dto.UnitId, cancellationToken);
                if (unit != null)
                    dto.UnitName = unit.UnitName;
            }

            // CostCenter enrichment
            if (string.IsNullOrWhiteSpace(dto.CostCenterName) && dto.CostCenterId > 0)
            {
                var cc = await _costCenterLookup.GetByIdAsync(dto.CostCenterId, cancellationToken);
                if (cc != null)
                    dto.CostCenterName = cc.CostCenterName;
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetBudgetGroupById",
                actionCode: dto.Id.ToString(),
                actionName: dto.Name,
                details: "Budget Group detail fetched.",
                module: "BudgetGroup");

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<BudgetGroupDto>
            {
                StatusCode = 200,
                IsSuccess = true,
                Message = "Success",
                Data = dto,
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 1
            };
        }
    }
}
