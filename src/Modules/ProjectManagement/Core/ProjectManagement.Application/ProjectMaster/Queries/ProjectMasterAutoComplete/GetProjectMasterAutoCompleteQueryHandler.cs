using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete
{
    public class GetProjectMasterAutoCompleteQueryHandler : IRequestHandler<GetProjectMasterAutoCompleteQuery, List<ProjectMasterAutoCompleteDto>>
    {
        private readonly IProjectMasterQueryRepository _repo;
        private readonly ILogger<GetProjectMasterAutoCompleteQueryHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly ICostCenterLookup _costCenterLookup;
        private readonly IAssetGroupLookup _assetGroupLookup;


        public GetProjectMasterAutoCompleteQueryHandler(
            IProjectMasterQueryRepository repo,
            ILogger<GetProjectMasterAutoCompleteQueryHandler> logger,
            IMediator mediator,
            IMapper mapper,
            ICurrencyLookup currencyLookup,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup,
            IFinancialYearLookup financialYearLookup,
            ICostCenterLookup costCenterLookup,
            IAssetGroupLookup assetGroupLookup
            )
        {
            _repo = repo;
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _currencyLookup = currencyLookup;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
            _financialYearLookup = financialYearLookup;
            _costCenterLookup = costCenterLookup;
            _assetGroupLookup = assetGroupLookup;
        }
        
        public async Task<List<ProjectMasterAutoCompleteDto>> Handle(
            GetProjectMasterAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Fetch from repository
            var projects = await _repo.GetProjectMasterAutoCompleteAsync(
                request.UnitId,
                request.DepartmentId,
                request.SearchTerm,
                request.Take,                
                cancellationToken);

            var result = _mapper.Map<List<ProjectMasterAutoCompleteDto>>(projects);

            if (result.Count == 0)
            {
                await PublishAuditEventAsync(request, cancellationToken);
                return result;
            }

            // 2️⃣ Collect distinct IDs
            var currencyIds = result
                .Where(p => p.CurrencyId > 0)
                .Select(p => p.CurrencyId)
                .Distinct()
                .ToList();
                

            var budgetYearIds = result
                .Where(p => p.BudgetYearId > 0)
                .Select(p => p.BudgetYearId)
                .Distinct()
                .ToList();

            var costCenterIds = result
                .Where(p => p.CostCenterId > 0)
                .Select(p => p.CostCenterId)
                .Distinct()
                .ToList();

            var departmentIds = result
                .Where(p => p.DepartmentId > 0)
                .Select(p => p.DepartmentId)
                .Distinct()
                .ToList();

            var unitIds = result
                .Where(p => p.UnitId > 0)
                .Select(p => p.UnitId)
                .Distinct()
                .ToList();

            var assetGroupIds = result             // ✅ only if DTO has AssetGroupId
                .Where(p => p.AssetGroupId > 0)
                .Select(p => p.AssetGroupId)
                .Distinct()
                .ToList();

            // 3️⃣ Kick off lookups in parallel

            // Departments & Units
            var deptTask = _departmentLookup.GetAllDepartmentAsync();
            var unitTask = _unitLookup.GetAllUnitAsync();

            // Currency (batched by Ids)
            Task<IReadOnlyList<CurrencyLookupDto>> currencyTask =
                Task.FromResult<IReadOnlyList<CurrencyLookupDto>>(new List<CurrencyLookupDto>());

            if (currencyIds.Any())
                currencyTask = _currencyLookup.GetByIdsAsync(currencyIds, cancellationToken);

            // Financial Year & CostCenter
            var finYearTask    = _financialYearLookup.GetAllFinancialYearAsync();
            var costCenterTask = _costCenterLookup.GetAllCostCentersAsync();

            // AssetGroup (batched by Ids) – same style as in GetProjectMasterQueryHandler
            Task<IReadOnlyList<AssetGroupLookupDto>> assetGroupTask =
                Task.FromResult<IReadOnlyList<AssetGroupLookupDto>>(new List<AssetGroupLookupDto>());

            if (assetGroupIds.Any())
                assetGroupTask = _assetGroupLookup.GetByIdsAsync(assetGroupIds, cancellationToken);

            await Task.WhenAll(
                deptTask,
                unitTask,
                currencyTask,
                finYearTask,
                costCenterTask,
                assetGroupTask);

            var departments    = await deptTask;
            var units          = await unitTask;
            var currencies     = await currencyTask;
            var financialYears = await finYearTask;
            var costCenters    = await costCenterTask;
            var assetGroups    = await assetGroupTask;

            // 4️⃣ Build lookup maps

            var deptMap = departments
                .Where(d => d.DepartmentId > 0)
                .ToDictionary(
                    d => d.DepartmentId,
                    d => d.DepartmentName ?? string.Empty);

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

            var assetGroupMap = assetGroups
                .Where(a => a.AssetGroupId > 0)
                .ToDictionary(
                    a => a.AssetGroupId,
                    a => a);

            // 5️⃣ Enrich autocomplete DTO rows

            foreach (var p in result)
            {
                // Department
                if (deptMap.TryGetValue(p.DepartmentId, out var deptName) &&
                    !string.IsNullOrWhiteSpace(deptName))
                {
                    p.DepartmentName = deptName;
                }

                // Currency
                if (p.CurrencyId > 0 &&
                    currencyMap.TryGetValue(p.CurrencyId, out var cur) &&
                    cur is not null)
                {
                    p.CurrencyName = !string.IsNullOrWhiteSpace(cur.Code)
                        ? cur.Code
                        : cur.Name;
                }

                // Financial Year (Budget Year)
                if (p.BudgetYearId > 0 &&
                    finYearMap.TryGetValue(p.BudgetYearId, out var finYearName) &&
                    !string.IsNullOrWhiteSpace(finYearName))
                {
                    p.BudgetYearName = finYearName;
                }

                // Cost Center
                if (p.CostCenterId > 0 &&
                    costCenterMap.TryGetValue(p.CostCenterId, out var ccName) &&
                    !string.IsNullOrWhiteSpace(ccName))
                {
                    p.CostCenterName = ccName;
                }

                // Asset Group
                if (p.AssetGroupId > 0 &&
                    assetGroupMap.TryGetValue(p.AssetGroupId, out var ag) &&
                    ag is not null &&
                    !string.IsNullOrWhiteSpace(ag.GroupName))
                {
                    p.AssetGroupName = ag.GroupName;
                }
            }

            // 6️⃣ Audit log
            await PublishAuditEventAsync(request, cancellationToken);

            return result;
        }

        private async Task PublishAuditEventAsync(
            GetProjectMasterAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode: "",
                actionName: "Project Master AutoComplete",
                details: $"Project master autocomplete fetched. UnitId={request.UnitId}, DepartmentId={request.DepartmentId}, Search='{request.SearchTerm}'",
                module: "ProjectMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
        }
        
      
    }
}
