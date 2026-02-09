using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.FixedAsset;
using Contracts.Dtos.Users;
using Contracts.Interfaces.External.IFixedAssetManagement;
using Contracts.Interfaces.External.IMaintenance;
using Contracts.Interfaces.External.IUser;
using ProjectManagement.Application.Common.HttpResponse;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Domain.Events;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster
{
    public class GetProjectMasterQueryHandler : IRequestHandler<GetProjectMasterQuery, ApiResponseDTO<List<GetProjectMasterDto>>>
    {

        private readonly IProjectMasterQueryRepository _projectMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrencyGrpcClient _currencyGrpcClient;
        private readonly IAssetGroupGrpcClient _assetGroupGrpcClient;
        private readonly IDepartmentGrpcClient _departmentGrpcClient;

        private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
        private readonly IUnitGrpcClient _unitGrpcClient;
        private readonly IFinancialYearGrpcClient _financialYearGrpcClient;
        private readonly ICostCenterGrpcClient _costCenterGrpcClient;


        public GetProjectMasterQueryHandler(IProjectMasterQueryRepository projectMasterQueryRepository, IMapper mapper, IMediator mediator, ICurrencyGrpcClient currencyGrpcClient, IAssetGroupGrpcClient assetGroupGrpcClient,
            IDepartmentGrpcClient departmentGrpcClient, IUnitGrpcClient unitGrpcClient, IFinancialYearGrpcClient financialYearGrpcClient, ICostCenterGrpcClient costCenterGrpcClient, IDepartmentAllGrpcClient departmentAllGrpcClient)
        {
            _projectMasterQueryRepository = projectMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _currencyGrpcClient = currencyGrpcClient;
            _assetGroupGrpcClient = assetGroupGrpcClient;
            _departmentGrpcClient = departmentGrpcClient;
            _unitGrpcClient = unitGrpcClient;
            _financialYearGrpcClient = financialYearGrpcClient;
            _costCenterGrpcClient = costCenterGrpcClient;
            _departmentAllGrpcClient = departmentAllGrpcClient;
        }
        
         public async Task<ApiResponseDTO<List<GetProjectMasterDto>>> Handle(
            GetProjectMasterQuery request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Get paged data from DB (Dapper)
            var (projects, totalCount) = await _projectMasterQueryRepository
                .GetProjectmasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var projectList = _mapper.Map<List<GetProjectMasterDto>>(projects);
            
       
            
            if (projectList.Count > 0)
            {
                // 2️⃣ Collect distinct IDs for batch gRPC lookups
                var currencyIds = projectList
                    .Where(p => p.CurrencyId > 0)
                    .Select(p => p.CurrencyId)
                    .Distinct()
                    .ToList();

                var assetGroupIds = projectList
                    .Where(p => p.AssetGroupId > 0)
                    .Select(p => p.AssetGroupId)
                    .Distinct()
                    .ToList();

                // 3️⃣ Kick off gRPC calls in parallel

                // Departments & Units
                //  var deptTask     = _departmentGrpcClient.GetAllDepartmentAsync();
                var deptTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
                var unitTask = _unitGrpcClient.GetAllUnitAsync();

                // Currency & AssetGroup (batched by Ids)
                Task<List<CurrencyDto>> currencyTask =
                    Task.FromResult(new List<CurrencyDto>());

                Task<IReadOnlyList<AssetGroupDto>> assetGroupTask =
                    Task.FromResult<IReadOnlyList<AssetGroupDto>>(new List<AssetGroupDto>());

                if (currencyIds.Any())
                    currencyTask = _currencyGrpcClient.GetByIdsAsync(currencyIds, cancellationToken);

                if (assetGroupIds.Any())
                    assetGroupTask = _assetGroupGrpcClient.GetByIdsAsync(assetGroupIds, cancellationToken);

                // Financial Year & CostCenter – you already call "GetAll" in spindle report
                var finYearTask = _financialYearGrpcClient.GetAllFinancialYearAsync();
                var costCenterTask = _costCenterGrpcClient.GetAllCostCentersAsync();

                await Task.WhenAll(
                    deptTask,
                    unitTask,
                    currencyTask,
                    assetGroupTask,
                    finYearTask,
                    costCenterTask);

                var departments = await deptTask;
                var units = await unitTask;
                var currencies = await currencyTask;
                var assetGroups = await assetGroupTask;
                var financialYears = await finYearTask;
                var costCenters = await costCenterTask;

                // 4️⃣ Build lookup dictionaries

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
                    .GroupBy(c => c.Id)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First());

                var assetGroupMap = assetGroups
                    .GroupBy(a => a.Id)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First());

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

                // 5️⃣ Enrich each ProjectMasterDto row
                foreach (var p in projectList)
                {
                    // Department
                    if (deptMap.TryGetValue(p.DepartmentId, out var deptName) &&
                        !string.IsNullOrWhiteSpace(deptName))
                    {
                        p.DepartmentName = deptName;
                    }

                    // Unit
                    if (unitMap.TryGetValue(p.UnitId, out var unitName) &&
                        !string.IsNullOrWhiteSpace(unitName))
                    {
                        p.UnitName = unitName;
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

                    // AssetGroup
                    if (p.AssetGroupId > 0 &&
                        assetGroupMap.TryGetValue(p.AssetGroupId, out var ag) &&
                        ag is not null &&
                        !string.IsNullOrWhiteSpace(ag.GroupName))
                    {
                        p.AssetGroup = ag.GroupName;
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


                }
            }

            // 6️⃣ Domain Event – for Audit Log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "ProjectMaster details were fetched.",
                module: "ProjectMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // 7️⃣ Wrap in ApiResponseDTO
            return new ApiResponseDTO<List<GetProjectMasterDto>>
            {
                IsSuccess  = true,              
                Message    = "Success",
                Data       = projectList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize   = request.PageSize
            };
        }
        
        // public async Task<ApiResponseDTO<List<ProjectMasterDto>>> Handle(
        //     GetProjectMasterQuery request,
        //     CancellationToken cancellationToken)
        // {


        //     // 1️⃣ Get paged data from DB (Dapper)
        //     var (projects, totalCount) = await _projectMasterQueryRepository
        //         .GetProjectmasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);

        //     var projectList = _mapper.Map<List<ProjectMasterDto>>(projects);

        //     if (projectList.Count > 0)
        //     {
        //         // 2️⃣ Collect distinct IDs for batch gRPC lookups
        //         var currencyIds = projectList.Where(p => p.CurrencyId > 0).Select(p => p.CurrencyId).Distinct().ToList();

        //         var assetGroupIds = projectList.Where(p => p.AssetGroupId > 0).Select(p => p.AssetGroupId).Distinct()
        //             .ToList();

        //         // 3️⃣ Kick off gRPC calls in parallel

        //         // Departments & Units (no ct if interface doesn’t support it)
        //         var deptTask = _departmentGrpcClient.GetAllDepartmentAsync();
        //         var unitTask = _unitGrpcClient.GetAllUnitAsync();

        //         Task<List<CurrencyDto>> currencyTask =
        //             Task.FromResult(new List<CurrencyDto>());

        //         Task<IReadOnlyList<AssetGroupDto>> assetGroupTask =
        //             Task.FromResult<IReadOnlyList<AssetGroupDto>>(new List<AssetGroupDto>());

        //         if (currencyIds.Any())
        //             currencyTask = _currencyGrpcClient.GetByIdsAsync(currencyIds, cancellationToken);

        //         if (assetGroupIds.Any())
        //             assetGroupTask = _assetGroupGrpcClient.GetByIdsAsync(assetGroupIds, cancellationToken);

        //         await Task.WhenAll(deptTask, unitTask, currencyTask, assetGroupTask);

        //         var departments  = await deptTask;
        //         var units        = await unitTask;
        //         var currencies   = await currencyTask;
        //         var assetGroups  = await assetGroupTask;

        //         // 4️⃣ Build lookup dictionaries

        //         var deptMap = departments
        //             .Where(d => d.DepartmentId > 0)
        //             .ToDictionary(
        //                 d => d.DepartmentId,
        //                 d => d.DepartmentName ?? string.Empty);

        //         var unitMap = units
        //             .Where(u => u.UnitId > 0)
        //             .ToDictionary(
        //                 u => u.UnitId,
        //                 u => u.UnitName ?? string.Empty);

        //         var currencyMap = currencies
        //             .GroupBy(c => c.Id)
        //             .ToDictionary(
        //                 g => g.Key,
        //                 g => g.First());

        //         var assetGroupMap = assetGroups
        //             .GroupBy(a => a.Id)
        //             .ToDictionary(
        //                 g => g.Key,
        //                 g => g.First());

        //         // 5️⃣ Enrich each ProjectMasterDto row
        //         foreach (var p in projectList)
        //         {
        //             // Department
        //             if (deptMap.TryGetValue(p.DepartmentId, out var deptName) &&
        //                 !string.IsNullOrWhiteSpace(deptName))
        //             {
        //                 p.DepartmentName = deptName;
        //             }

        //             // Unit
        //             if (unitMap.TryGetValue(p.UnitId, out var unitName) &&
        //                 !string.IsNullOrWhiteSpace(unitName))
        //             {
        //                 p.UnitName = unitName;
        //             }

        //             // Currency
        //             if (p.CurrencyId > 0 &&
        //                 currencyMap.TryGetValue(p.CurrencyId, out var cur) &&
        //                 cur is not null)
        //             {
        //                 p.CurrencyName = !string.IsNullOrWhiteSpace(cur.Code)
        //                     ? cur.Code
        //                     : cur.Name;
        //             }

        //             // AssetGroup
        //             if (p.AssetGroupId > 0 &&
        //                 assetGroupMap.TryGetValue(p.AssetGroupId, out var ag) &&
        //                 ag is not null &&
        //                 !string.IsNullOrWhiteSpace(ag.GroupName))
        //             {
        //                 p.AssetGroup = ag.GroupName;
        //             }
        //         }
        //     }

        //     // 6️⃣ Domain Event – for Audit Log
        //     var domainEvent = new AuditLogsDomainEvent(
        //         actionDetail: "GetAll",
        //         actionCode: "",
        //         actionName: "",
        //         details: "ProjectMaster details were fetched.",
        //         module: "ProjectMaster"
        //     );

        //     await _mediator.Publish(domainEvent, cancellationToken);

        //     // 7️⃣ Wrap in ApiResponseDTO
        //     return new ApiResponseDTO<List<ProjectMasterDto>>
        //     {
        //         IsSuccess  = true,
        //         Message    = "Success",
        //         Data       = projectList,
        //         TotalCount = totalCount,
        //         PageNumber = request.PageNumber,
        //         PageSize   = request.PageSize
        //     };
        // }


    }
}