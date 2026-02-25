using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Common;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleDetailsMonthwise
{
    public class GetSpindleDetailsMonthwiseQueryhandler : IRequestHandler<GetSpindleDetailsMonthwiseQuery, ApiResponseDTO<List<GetSpindleDetailsMonthwiseDto>>>
    {
        private readonly IBudgetAllocationQueryRepository _ibudgetAllocationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly ICostCenterLookup _costCenterLookup;

        public GetSpindleDetailsMonthwiseQueryhandler(IBudgetAllocationQueryRepository ibudgetAllocationQueryRepository, IMapper mapper, IMediator mediator, IUnitLookup unitLookup, IDepartmentLookup departmentLookup, ICurrencyLookup currencyLookup, ICostCenterLookup costCenterLookup)
        {
            _ibudgetAllocationQueryRepository = ibudgetAllocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
            _currencyLookup = currencyLookup;
            _costCenterLookup = costCenterLookup;
        }

        public async Task<ApiResponseDTO<List<GetSpindleDetailsMonthwiseDto>>> Handle(GetSpindleDetailsMonthwiseQuery request, CancellationToken cancellationToken)
        {
            var (result, totalCount) = await _ibudgetAllocationQueryRepository.GetBudgetGroupDetailSpindlewise(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm);

            var budgetGroupList = _mapper.Map<List<GetSpindleDetailsMonthwiseDto>>(result);
           var unitTask = _unitLookup.GetAllUnitAsync();
           var departmentTask = _departmentLookup.GetAllDepartmentAsync();
           var currencyIds = budgetGroupList
                    .Select(x => x.CurrencyId)
                    .Distinct()
                    .ToList();
           var currencyTask = _currencyLookup.GetByIdsAsync(currencyIds, cancellationToken);
           var costcenterTask = _costCenterLookup.GetAllCostCentersAsync();


            await Task.WhenAll(unitTask, departmentTask, currencyTask, costcenterTask);

            // Dictionary → Key: UnitId, Value: { UnitName, SpindlesCapacity }
            var unitLookup = (await unitTask)
                            .ToDictionary(
                                u => u.UnitId,
                                u => new { u.UnitName, u.SpindlesCapacity }
                            );

            // Dictionary → Key: DepartmentId, Value: DepartmentName
            var departmentLookup = (await departmentTask)
                                    .ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            // Dictionary → Key: CurrencyId, Value: CurrencyName
            var currencyLookup = (await currencyTask)
                                    .ToDictionary(c => c.CurrencyId, c => c.Code);

            // Dictionary → Key: CostCenterId, Value: CostCenterName
            var costcenterLookup = (await costcenterTask)
                                    .ToDictionary(c => c.CostCenterId, c => c.CostCenterName);

            foreach (var dto in budgetGroupList)
            {
                // Fetch UnitName + SpindlesCapacity
                if (unitLookup.TryGetValue(dto.UnitId, out var unitInfo))
                {
                    dto.UnitName = unitInfo.UnitName;
                    dto.SpindleCount = unitInfo.SpindlesCapacity ?? 0;
                    // Correct approved amount calculation
                    dto.ApprovedAmount = dto.SpindleCount * dto.RatePerSpindle;
                }

                // Fetch DepartmentName
                if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                {
                    dto.DepartmentName = deptName;
                }
                // Fetch DepartmentName
                if (currencyLookup.TryGetValue(dto.CurrencyId, out var currencyCode))
                {
                    dto.CurrencyName = currencyCode;
                }
                else
                {
                    dto.CurrencyName = "";
                }

                // Fetch CostCenterName
                if (costcenterLookup.TryGetValue(dto.CostCenterId, out var costcenterName))
                {
                    dto.CostCenter = costcenterName;
                }
                else
                {
                    dto.CostCenter = "";
                }
            }
             await _mediator.Publish(
                    new AuditLogsDomainEvent(
                        actionDetail: "GetAll",
                        actionCode: nameof(GetSpindleDetailsMonthwiseQuery),
                        actionName: budgetGroupList.Count.ToString(),
                        details:  $"Budget details was fetched.",
                        module: "Budget Allocation"
                    ),
                    cancellationToken
                );
                        
            return new ApiResponseDTO<List<GetSpindleDetailsMonthwiseDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = budgetGroupList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

        }
    }
}