using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Common;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroup
{
    public class GetBudgetGroupQueryHandler
        : IRequestHandler<GetBudgetGroupQuery, ApiResponseDTO<List<BudgetGroupListItemDto>>>
    {
        private readonly IBudgetGroupQueryRepository _budgetGroupQueryRepository;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ICostCenterLookup _costCenterLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public GetBudgetGroupQueryHandler(
            IBudgetGroupQueryRepository budgetGroupQueryRepository,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup,
            ICostCenterLookup costCenterLookup,
            ICurrencyLookup currencyLookup)
        {
            _budgetGroupQueryRepository = budgetGroupQueryRepository;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
            _costCenterLookup = costCenterLookup;
            _currencyLookup = currencyLookup;
        }

        public async Task<ApiResponseDTO<List<BudgetGroupListItemDto>>> Handle(
            GetBudgetGroupQuery request,
            CancellationToken cancellationToken)
        {
            var filter = new BudgetGroupListFilterDto
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SearchTerm = request.SearchTerm,
                UnitId = request.UnitId,
                DepartmentId = request.DepartmentId,
                CostCenterId = request.CostCenterId,
                ParentBudgetGroupId = request.ParentBudgetGroupId,
                IsActive = request.IsActive,
                AllocationRuleId = request.AllocationRuleId,
                BudgetTypeId = request.BudgetTypeId,
                CarryForward = request.CarryForward
            };

            var (items, totalCount) = await _budgetGroupQueryRepository
                .GetAllAsync(filter, cancellationToken);

            // Department and Unit enrichment
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var deptLookupDict = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var unitLookupDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var dto in items)
            {
                if (string.IsNullOrWhiteSpace(dto.DepartmentName) &&
                    deptLookupDict.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.DepartmentName = deptName ?? string.Empty;

                if (string.IsNullOrWhiteSpace(dto.UnitName) &&
                    unitLookupDict.TryGetValue(dto.UnitId, out var unitName))
                    dto.UnitName = unitName;
            }

            // CostCenter enrichment
            var costCenters = await _costCenterLookup.GetAllCostCentersAsync();
            var ccLookup = costCenters.ToDictionary(c => c.CostCenterId, c => c.CostCenterName);

            foreach (var dto in items)
            {
                if (dto.CostCenterId > 0 &&
                    ccLookup.TryGetValue(dto.CostCenterId, out var ccName) &&
                    string.IsNullOrWhiteSpace(dto.CostCenterName))
                    dto.CostCenterName = ccName ?? string.Empty;
            }

            // Currency enrichment
            var currencyIds = items
                .Where(x => x.CurrencyId > 0)
                .Select(x => x.CurrencyId)
                .Distinct()
                .ToList();

            if (currencyIds.Count > 0)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(currencyIds, cancellationToken);
                var currencyLookupDict = currencies.ToDictionary(x => x.CurrencyId, x => x.Name);

                foreach (var dto in items)
                {
                    if (dto.CurrencyId > 0 &&
                        string.IsNullOrWhiteSpace(dto.CurrencyName) &&
                        currencyLookupDict.TryGetValue(dto.CurrencyId, out var curName))
                    {
                        dto.CurrencyName = curName;
                    }
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetBudgetGroup",
                actionCode: "Get",
                actionName: items.Count.ToString(),
                details: "Budget Group list fetched.",
                module: "BudgetGroup");

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<BudgetGroupListItemDto>>
            {
                IsSuccess  = true,
                StatusCode = 200,
                Message    = "Success",
                Data       = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize   = request.PageSize
            };
        }
    }
}
