using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleMonthwiseReport
{
    public class GetSpindleMonthwiseReportQueryHandler : IRequestHandler<GetSpindleMonthwiseReportQuery, List<GetSpindleMonthwiseReportDto>>
    {
        private readonly IBudgetAllocationQueryRepository _ibudgetAllocationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly ICostCenterLookup _costCenterLookup;

        public GetSpindleMonthwiseReportQueryHandler(IBudgetAllocationQueryRepository ibudgetAllocationQueryRepository, IMapper mapper, IMediator mediator, IUnitLookup unitLookup, IDepartmentLookup departmentLookup, ICurrencyLookup currencyLookup, IFinancialYearLookup financialYearLookup, ICostCenterLookup costCenterLookup)
        {
            _ibudgetAllocationQueryRepository = ibudgetAllocationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
            _currencyLookup = currencyLookup;
            _financialYearLookup = financialYearLookup;
            _costCenterLookup = costCenterLookup;
        }

        public async Task<List<GetSpindleMonthwiseReportDto>> Handle(
        GetSpindleMonthwiseReportQuery request, 
        CancellationToken cancellationToken)
    {
        // Step 1: Get SQL data
        var sqlData = await _ibudgetAllocationQueryRepository
            .GetSpindleDetailsMonthwiseAsync(
                request.FinancialYearId,
                request.DepartmentId,
                request.CostCenterId,request.AllocationTypeId,request.BudgetGroupId,request.BudgetDate
            );

        var pending = _mapper.Map<List<GetSpindleMonthwiseReportDto>>(sqlData);

        if (!pending.Any())
            return pending;

        // Step 2: Fire all lookup calls in parallel
        var unitTask = _unitLookup.GetAllUnitAsync();
        var deptTask = _departmentLookup.GetAllDepartmentAsync();
        var currencyIds = pending.Select(x => x.CurrencyId).Distinct().ToList();
        var currencyTask = _currencyLookup.GetByIdsAsync(currencyIds, cancellationToken);
        var finYearTask = _financialYearLookup.GetAllFinancialYearAsync();
        var costcenterTask = _costCenterLookup.GetAllCostCentersAsync();

        await Task.WhenAll(unitTask, deptTask, currencyTask, finYearTask, costcenterTask);

        // Step 3: Convert to lookups
        var unitLookupDict = (await unitTask)
            .ToDictionary(x => x.UnitId, x => x.UnitName);

        var deptLookupDict = (await deptTask)
            .ToDictionary(x => x.DepartmentId, x => x.DepartmentName);

        var currencyLookupDict = (await currencyTask)
            .ToDictionary(x => x.CurrencyId, x => x.Code);

        var financialYearLookupDict = (await finYearTask)
            .ToDictionary(x => x.FinancialYearId, x => x.FinancialYearName);

        var costcenterLookupDict = (await costcenterTask)
            .ToDictionary(c => c.CostCenterId, c => c.CostCenterName);

            // Step 4: Map lookup values to DTO
            foreach (var dto in pending)
            {
                if (unitLookupDict.TryGetValue(dto.UnitId, out var unitName))
                    dto.UnitName = unitName;

                if (deptLookupDict.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.Department = deptName;

                if (currencyLookupDict.TryGetValue(dto.CurrencyId, out var currencyName))
                    dto.Currency = currencyName;

                if (financialYearLookupDict.TryGetValue(dto.FinancialYearId, out var finYearName))
                    dto.FinancialYearName = finYearName;

                if (costcenterLookupDict.TryGetValue(dto.CostCenterId, out var costcenterName))
                    dto.CostCenter = costcenterName;
            }

        // Step 5: Publish audit log
        await _mediator.Publish(new AuditLogsDomainEvent(
            actionDetail: "GetSpindleMonthwiseReportQuery",
            actionCode: "GetSpindleMonthwiseReportQuery",
            actionName: pending.Count.ToString(),
            details: "GetSpindleMonthwiseReportQuery",
            module: "BudgetAllocation"
        ), cancellationToken);

        return pending;
    }
    }
}