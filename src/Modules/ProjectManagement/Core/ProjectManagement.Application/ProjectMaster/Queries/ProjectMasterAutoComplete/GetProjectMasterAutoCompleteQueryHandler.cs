using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.FixedAsset;
using Contracts.Dtos.Users;
using Contracts.Interfaces.External.IFixedAssetManagement;
using Contracts.Interfaces.External.IMaintenance;
using Contracts.Interfaces.External.IUser;
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
        private readonly ICurrencyGrpcClient _currencyGrpcClient;
        private readonly IDepartmentGrpcClient _departmentGrpcClient;
        private readonly IUnitGrpcClient _unitGrpcClient;
        private readonly IFinancialYearGrpcClient _financialYearGrpcClient;
        private readonly ICostCenterGrpcClient _costCenterGrpcClient;
        private readonly IAssetGroupGrpcClient _assetGroupGrpcClient;


        public GetProjectMasterAutoCompleteQueryHandler(
            IProjectMasterQueryRepository repo,
            ILogger<GetProjectMasterAutoCompleteQueryHandler> logger,
            IMediator mediator,
            IMapper mapper,
            ICurrencyGrpcClient currencyGrpcClient,
            IDepartmentGrpcClient departmentGrpcClient,
            IUnitGrpcClient unitGrpcClient,
            IFinancialYearGrpcClient financialYearGrpcClient,
            ICostCenterGrpcClient costCenterGrpcClient,
            IAssetGroupGrpcClient assetGroupGrpcClient
            )
        {
            _repo = repo;
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _currencyGrpcClient = currencyGrpcClient;
            _departmentGrpcClient = departmentGrpcClient;
            _unitGrpcClient = unitGrpcClient;
            _financialYearGrpcClient = financialYearGrpcClient;
            _costCenterGrpcClient = costCenterGrpcClient;
            _assetGroupGrpcClient = assetGroupGrpcClient;
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

            // 3️⃣ Kick off gRPC in parallel

            // Departments & Units
            var deptTask = _departmentGrpcClient.GetAllDepartmentAsync();
            var unitTask = _unitGrpcClient.GetAllUnitAsync();

            // Currency (batched by Ids)
            Task<List<CurrencyDto>> currencyTask =
                Task.FromResult(new List<CurrencyDto>());

            if (currencyIds.Any())
                currencyTask = _currencyGrpcClient.GetByIdsAsync(currencyIds, cancellationToken);

            // Financial Year & CostCenter
            var finYearTask    = _financialYearGrpcClient.GetAllFinancialYearAsync();
            var costCenterTask = _costCenterGrpcClient.GetAllCostCentersAsync();

            // AssetGroup (batched by Ids) – same style as in GetProjectMasterQueryHandler
            Task<IReadOnlyList<AssetGroupDto>> assetGroupTask =
                Task.FromResult<IReadOnlyList<AssetGroupDto>>(new List<AssetGroupDto>());

            if (assetGroupIds.Any())
                assetGroupTask = _assetGroupGrpcClient.GetByIdsAsync(assetGroupIds, cancellationToken);

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
                .Where(c => c.Id > 0)
                .ToDictionary(
                    c => c.Id,          // int key
                    c => c);

            var finYearMap = financialYears
                .Where(f => f.Id > 0)
                .ToDictionary(
                    f => f.Id,
                    f => f.FinYearName ?? string.Empty);

            var costCenterMap = costCenters
                .Where(c => c.Id > 0)
                .ToDictionary(
                    c => c.Id,
                    c => c.CostCenterName ?? string.Empty);

            var assetGroupMap = assetGroups
                .GroupBy(a => a.Id)
                .ToDictionary(
                    g => g.Key,
                    g => g.First());

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
        
        //  public async Task<List<ProjectMasterAutoCompleteDto>> Handle(
        //     GetProjectMasterAutoCompleteQuery request,
        //     CancellationToken cancellationToken)
        // {

        //     // 1️⃣ Fetch from repository (entity or lightweight projection)
        //     var projects = await _repo.GetProjectMasterAutoCompleteAsync(
        //         request.UnitId,
        //         request.DepartmentId,
        //         request.SearchTerm, 
        //         request.Take,           
        //         cancellationToken);

        //     // 2️⃣ If repo returns entities, map them
        //     //    If repo already returns ProjectMasterAutoCompleteDto, just skip this line and use `projects` directly.
        //     var result = _mapper.Map<List<ProjectMasterAutoCompleteDto>>(projects);

        //     // 3️⃣ Domain Event – audit log
        //     var domainEvent = new AuditLogsDomainEvent(
        //         actionDetail: "GetAutoComplete",
        //         actionCode: "",
        //         actionName: "Project Master AutoComplete",
        //         details: $"Project master autocomplete fetched. UnitId={request.UnitId}, DepartmentId={request.DepartmentId}, Search='{request.SearchTerm}'",
        //         module: "ProjectMaster"
        //     );

        //     await _mediator.Publish(domainEvent, cancellationToken);

        //     return result;
        // }


    }
}