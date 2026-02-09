using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Users;
using Contracts.Interfaces.External.IFixedAssetManagement;
using Contracts.Interfaces.External.IMaintenance;
using Contracts.Interfaces.External.IUser;
using ProjectManagement.Application.Common.HttpResponse;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Domain.Events;
using MediatR;

namespace ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetAllProjectWBS
{
    public class GetAllprojectWBSQueryHandler : IRequestHandler<GetAllprojectWBSQuery, ApiResponseDTO<List<ProjectWorkBreakdownStructureDto>>>
    {
        private readonly IProjectWorkBreakdownStructureQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ICurrencyGrpcClient _currencyGrpcClient;
        private readonly IUnitGrpcClient _unitGrpcClient;
        private readonly IFinancialYearGrpcClient _financialYearGrpcClient;
        private readonly ICostCenterGrpcClient _costCenterGrpcClient;
         private readonly IDepartmentGrpcClient _departmentGrpcClient;

        private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;

        public GetAllprojectWBSQueryHandler(
            IProjectWorkBreakdownStructureQueryRepository queryRepository,
            ICurrencyGrpcClient currencyGrpcClient,
            IUnitGrpcClient unitGrpcClient,
            IFinancialYearGrpcClient financialYearGrpcClient,
            ICostCenterGrpcClient costCenterGrpcClient,
            IMapper mapper,
            IMediator mediator,
            IDepartmentGrpcClient departmentGrpcClient,
            IDepartmentAllGrpcClient departmentAllGrpcClient
            )
        {
            _queryRepository = queryRepository;
            _currencyGrpcClient = currencyGrpcClient;
            _unitGrpcClient = unitGrpcClient;
            _financialYearGrpcClient = financialYearGrpcClient;
            _costCenterGrpcClient = costCenterGrpcClient;
            _mapper = mapper;
            _mediator = mediator;
            _departmentGrpcClient = departmentGrpcClient;
            _departmentAllGrpcClient = departmentAllGrpcClient;
        }
        
            public async Task<ApiResponseDTO<List<ProjectWorkBreakdownStructureDto>>> Handle(
            GetAllprojectWBSQuery request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Get paged WBS data from DB
            var (items, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm);

            var wbsList = items.ToList();

            if (wbsList.Count > 0)
            {
                // 2️⃣ Collect distinct IDs for batch gRPC lookups


                var unitIds = wbsList
                    .Where(x => x.UnitId > 0)
                    .Select(x => x.UnitId)
                    .Distinct()
                    .ToList();

                var currencyIds = wbsList
                    .Where(x => x.CurrencyId > 0)
                    .Select(x => x.CurrencyId)
                    .Distinct()
                    .ToList();

          

                var budgetYearIds = wbsList
                    .Where(x => x.BudgetYearId > 0)
                    .Select(x => x.BudgetYearId)
                    .Distinct()
                    .ToList();

                var costCenterIds = wbsList
                    .Where(x => x.CostCenterId > 0)
                    .Select(x => x.CostCenterId)
                    .Distinct()
                    .ToList();

                // 3️⃣ Kick off gRPC calls in parallel

          
              var deptTask = _departmentAllGrpcClient.GetDepartmentAllAsync();
                var unitTask = _unitGrpcClient.GetAllUnitAsync();
                // Currency & AssetGroup – batched by IDs
                Task<List<CurrencyDto>> currencyTask =
                    Task.FromResult(new List<CurrencyDto>());              
                if (currencyIds.Any())
                    currencyTask = _currencyGrpcClient.GetByIdsAsync(currencyIds, cancellationToken);             

                // Financial Year & CostCenter – usually GetAll
                var finYearTask    = _financialYearGrpcClient.GetAllFinancialYearAsync();
                var costCenterTask = _costCenterGrpcClient.GetAllCostCentersAsync();

                await Task.WhenAll(
                   
                    unitTask,
                    currencyTask,                 
                    finYearTask,
                    costCenterTask,
                    deptTask);

               
                var units          = await unitTask;
                var currencies     = await currencyTask;               
                var financialYears = await finYearTask;
                var costCenters    = await costCenterTask;
                 var departments = await deptTask;

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

                // 5️⃣ Enrich each WBS row
                foreach (var w in wbsList)
                {

                      // Department
                    if (deptMap.TryGetValue(w.ResponsibleDepartmentId, out var deptName) &&
                        !string.IsNullOrWhiteSpace(deptName))
                    {
                        w.ResponsibleDepartment = deptName;
                    }

                    // Unit
                    if (w.UnitId > 0 &&
                        unitMap.TryGetValue(w.UnitId, out var unitName) &&
                        !string.IsNullOrWhiteSpace(unitName))
                    {
                        w.UnitName = unitName;
                    }

                    // Currency
                    if (w.CurrencyId > 0 &&
                        currencyMap.TryGetValue(w.CurrencyId, out var cur) &&
                        cur is not null)
                    {
                        w.CurrencyName = !string.IsNullOrWhiteSpace(cur.Code)
                            ? cur.Code
                            : cur.Name;
                    }


                    // Financial / Budget Year
                    if (w.BudgetYearId > 0 &&
                        finYearMap.TryGetValue(w.BudgetYearId, out var finYearName) &&
                        !string.IsNullOrWhiteSpace(finYearName))
                    {
                        w.BudgetYearName = finYearName;
                    }
                    

                    
                     // Cost Center
                    if (w.CostCenterId.HasValue && w.CostCenterId.Value > 0 &&
                        costCenterMap.TryGetValue(w.CostCenterId.Value, out var costCenterName) &&
                        !string.IsNullOrWhiteSpace(costCenterName))
                    {
                        w.CostCenterName = costCenterName;
                    }

                    
                }
            }

            // 6️⃣ Domain Event – for Audit Log (similar to ProjectMaster)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Project Work Breakdown Structure list fetched.",
                module: "ProjectWBS"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // 7️⃣ Wrap in ApiResponseDTO
            return new ApiResponseDTO<List<ProjectWorkBreakdownStructureDto>>
            {
                IsSuccess  = true,
                Message    = "Project WBS list fetched successfully.",
                Data       = wbsList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize   = request.PageSize
            };
        }
    }
}
