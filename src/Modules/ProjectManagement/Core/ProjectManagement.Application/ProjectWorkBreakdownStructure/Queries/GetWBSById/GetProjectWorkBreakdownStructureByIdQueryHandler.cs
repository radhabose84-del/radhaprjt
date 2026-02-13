using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetById;
using ProjectManagement.Domain.Events;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWBSById
{
    public class GetProjectWorkBreakdownStructureByIdQueryHandler    : IRequestHandler<GetProjectWorkBreakdownStructureByIdQuery, ProjectWorkBreakdownStructureDto?>
    {
        private readonly IProjectWorkBreakdownStructureQueryRepository _queryRepository;
        
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly ICostCenterLookup _costCenterLookup;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;


        public GetProjectWorkBreakdownStructureByIdQueryHandler(
            IProjectWorkBreakdownStructureQueryRepository queryRepository,
            ICurrencyLookup currencyLookup,
            IUnitLookup unitLookup,
            IFinancialYearLookup financialYearLookup,
            ICostCenterLookup costCenterLookup,
            IMediator mediator,
            IDepartmentLookup departmentLookup)
        {
            _queryRepository = queryRepository;
            _currencyLookup = currencyLookup;
            _unitLookup = unitLookup;
            _financialYearLookup = financialYearLookup;
            _costCenterLookup = costCenterLookup;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }

        public async Task<ProjectWorkBreakdownStructureDto?> Handle(
            GetProjectWorkBreakdownStructureByIdQuery request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Dapper repo call
            var wbs = await _queryRepository.GetByIdAsync(request.Id);

            if (wbs is null)
            {
                // Controller can return 404 if this is null
                return null;
            }

            // If no related IDs, just return quickly
            var hasAnyRelatedId =             
                wbs.UnitId         > 0 ||
                wbs.CurrencyId     > 0 ||              
                wbs.BudgetYearId   > 0 ||
                wbs.ResponsibleDepartmentId   > 0 ||
                wbs.CostCenterId   > 0;

            if (hasAnyRelatedId)
            {
                // 2️⃣ Kick off gRPC calls in parallel

                // Departments & Units (GetAll pattern)
                var deptTask = _departmentLookup.GetAllDepartmentAsync();
                var unitTask = _unitLookup.GetAllUnitAsync();

                // Currency & AssetGroup – batched with single ID
                Task<IReadOnlyList<CurrencyLookupDto>> currencyTask =
                    Task.FromResult<IReadOnlyList<CurrencyLookupDto>>(Array.Empty<CurrencyLookupDto>());

               

                if (wbs.CurrencyId > 0)
                {
                    currencyTask = _currencyLookup.GetByIdsAsync(
                        new[] { wbs.CurrencyId }, cancellationToken);
                }

         

                // Financial Year & CostCenter
                var finYearTask    = _financialYearLookup.GetAllFinancialYearAsync();
                var costCenterTask = _costCenterLookup.GetAllCostCentersAsync();

                await Task.WhenAll(
                   
                    unitTask,
                    currencyTask,
                    deptTask,
                    finYearTask,
                    costCenterTask);

               
                var units          = await unitTask;
                var currencies     = await currencyTask;                
                var financialYears = await finYearTask;
                var costCenters    = await costCenterTask;
                var departments       = await deptTask;

                // 3️⃣ Build lookup dictionaries

                 var deptMap = departments                    
                    .Where(d => d.DepartmentId > 0)
                    .GroupBy(d => d.DepartmentId)
                    .ToDictionary(g => g.Key, g => g.First().DepartmentName ?? string.Empty);

                var unitMap = units
                    .Where(u => u.UnitId > 0)
                    .ToDictionary(
                        u => u.UnitId,
                        u => u.UnitName ?? string.Empty);

                var currencyMap = currencies
                    .Where(c => c.CurrencyId > 0)
                    .ToDictionary(
                        c => c.CurrencyId,
                        c => c);

     

                var finYearMap = financialYears
                    .Where(f => f.FinancialYearId > 0)
                    .ToDictionary(
                        f => f.FinancialYearId,
                        f => f.FinancialYearName ?? string.Empty);

                var costCenterMap = costCenters
                    .Where(c => c.CostCenterId > 0)
                    .ToDictionary(
                        c => c.CostCenterId,
                        c => c.CostCenterName ?? string.Empty);
                          
                // ✅ Responsible Department (int, NOT nullable)
                if (wbs.ResponsibleDepartmentId > 0 &&
                    deptMap.TryGetValue(wbs.ResponsibleDepartmentId, out var deptName) &&
                    !string.IsNullOrWhiteSpace(deptName))
                {
                    wbs.ResponsibleDepartment = deptName;
                }
                // Unit
                if (wbs.UnitId > 0 &&
                    unitMap.TryGetValue(wbs.UnitId, out var unitName) &&
                    !string.IsNullOrWhiteSpace(unitName))
                {
                    wbs.UnitName = unitName;
                }

                // Currency
                if (wbs.CurrencyId > 0 &&
                    currencyMap.TryGetValue(wbs.CurrencyId, out var cur) &&
                    cur is not null)
                {
                    wbs.CurrencyName = !string.IsNullOrWhiteSpace(cur.Code)
                        ? cur.Code
                        : cur.Name;
                }

              

                // Budget / Financial Year
                if (wbs.BudgetYearId > 0 &&
                    finYearMap.TryGetValue(wbs.BudgetYearId, out var finYearName) &&
                    !string.IsNullOrWhiteSpace(finYearName))
                {
                    wbs.BudgetYearName = finYearName;
                }

                // Cost Center
                // Cost Center
                    if (wbs.CostCenterId.HasValue &&
                        wbs.CostCenterId.Value > 0 &&
                        costCenterMap.TryGetValue(wbs.CostCenterId.Value, out var ccName) &&
                        !string.IsNullOrWhiteSpace(ccName))
                    {
                        wbs.CostCenterName = ccName;
                    }
            }

            // 5️⃣ Domain Event – Audit log (optional)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "",
                actionName: "",
                details: $"Project WBS details fetched. WBS Id: {request.Id}",
                module: "ProjectWBS"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return wbs;
        }
    }
}
