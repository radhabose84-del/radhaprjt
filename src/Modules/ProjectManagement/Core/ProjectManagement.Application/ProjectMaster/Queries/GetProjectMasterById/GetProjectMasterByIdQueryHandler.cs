using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Domain.Events;
using MediatR;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMasterById
{
    public class GetProjectMasterByIdQueryHandler : IRequestHandler<GetProjectMasterByIdQuery, GetProjectMasterDto>
    {
        private readonly IProjectMasterQueryRepository _projectMasterQueryRepository;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IAssetGroupLookup _assetGroupLookup;
        private readonly ICostCenterLookup _costCenterLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProjectMasterByIdQueryHandler(IProjectMasterQueryRepository projectMasterQueryRepository, IDepartmentLookup departmentLookup, IUnitLookup unitLookup, IMapper mapper, IMediator mediator
        , ICurrencyLookup currencyLookup, IAssetGroupLookup assetGroupLookup, ICostCenterLookup costCenterLookup, IFinancialYearLookup financialYearLookup)
        {
            _projectMasterQueryRepository = projectMasterQueryRepository;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
            _mapper = mapper;
            _mediator = mediator;
            _currencyLookup = currencyLookup;
            _assetGroupLookup = assetGroupLookup;
            _costCenterLookup = costCenterLookup;
            _financialYearLookup = financialYearLookup;
        }
        
          public async Task<GetProjectMasterDto> Handle(
            GetProjectMasterByIdQuery request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ Get entity from DB
            var entity = await _projectMasterQueryRepository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                // Replace with your custom exception (EntityNotFoundException, etc.)
                throw new Exception($"ProjectMaster with Id {request.Id} not found.");
            }

            // 2️⃣ Map entity → DTO via AutoMapper
            var project = _mapper.Map<GetProjectMasterDto>(entity);

            // Parallel lookup calls
            var deptTask = project.DepartmentId > 0
                ? _departmentLookup.GetByIdAsync(project.DepartmentId, cancellationToken)
                : Task.FromResult<DepartmentLookupDto?>(null);

            var unitTask = project.UnitId > 0
                ? _unitLookup.GetByIdAsync(project.UnitId, cancellationToken)
                : Task.FromResult<UnitLookupDto?>(null);

            Task<IReadOnlyList<CurrencyLookupDto>> currencyTask =
                Task.FromResult<IReadOnlyList<CurrencyLookupDto>>(Array.Empty<CurrencyLookupDto>());
            if (project.CurrencyId > 0)
                currencyTask = _currencyLookup.GetByIdsAsync(new[] { project.CurrencyId }, cancellationToken);

            Task<IReadOnlyList<AssetGroupLookupDto>> assetGroupTask =
                Task.FromResult<IReadOnlyList<AssetGroupLookupDto>>(Array.Empty<AssetGroupLookupDto>());
            if (project.AssetGroupId > 0)
                assetGroupTask = _assetGroupLookup.GetByIdsAsync(new[] { project.AssetGroupId }, cancellationToken);

            var finYearTask = project.BudgetYearId > 0
                ? _financialYearLookup.GetByIdAsync(project.BudgetYearId, cancellationToken)
                : Task.FromResult<FinancialYearLookupDto?>(null);

            var costCenterTask = project.CostCenterId > 0
                ? _costCenterLookup.GetByIdAsync(project.CostCenterId, cancellationToken)
                : Task.FromResult<CostCenterLookupDto?>(null);

            await Task.WhenAll(deptTask, unitTask, currencyTask, assetGroupTask, finYearTask, costCenterTask);

            var department = await deptTask;
            var unit = await unitTask;
            var currencies = await currencyTask;
            var assetGroups = await assetGroupTask;
            var financialYear = await finYearTask;
            var costCenter = await costCenterTask;

            if (department is not null && !string.IsNullOrWhiteSpace(department.DepartmentName))
                project.DepartmentName = department.DepartmentName;

            if (unit is not null && !string.IsNullOrWhiteSpace(unit.UnitName))
                project.UnitName = unit.UnitName;

            var currency = currencies.FirstOrDefault();
            if (currency is not null)
            {
                project.CurrencyName = !string.IsNullOrWhiteSpace(currency.Code)
                    ? currency.Code
                    : currency.Name;
            }

            var assetGroup = assetGroups.FirstOrDefault();
            if (assetGroup is not null && !string.IsNullOrWhiteSpace(assetGroup.GroupName))
                project.AssetGroup = assetGroup.GroupName;

            if (financialYear is not null && !string.IsNullOrWhiteSpace(financialYear.FinancialYearName))
                project.BudgetYearName = financialYear.FinancialYearName;

            if (costCenter is not null && !string.IsNullOrWhiteSpace(costCenter.CostCenterName))
                project.CostCenterName = costCenter.CostCenterName;

            // 7️⃣ Domain event – Audit log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetProjectMasterByIdQuery",
                actionName: project.Id.ToString(),
                details: $"ProjectMaster details {project.Id} were fetched.",
                module: "ProjectMaster"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return project;
        }        
       
    }
}
