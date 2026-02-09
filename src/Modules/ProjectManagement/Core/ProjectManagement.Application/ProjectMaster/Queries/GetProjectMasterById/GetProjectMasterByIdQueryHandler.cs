// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Dtos.FixedAsset;
// using Contracts.Dtos.Users;
// using Contracts.Interfaces.External.IFixedAssetManagement;
// using Contracts.Interfaces.External.IMaintenance;
// using Contracts.Interfaces.External.IUser;
// using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
// using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
// using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
// using ProjectManagement.Domain.Events;
// using MediatR;

// namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMasterById
// {
//     public class GetProjectMasterByIdQueryHandler : IRequestHandler<GetProjectMasterByIdQuery, GetProjectMasterDto>
//     {
//         private readonly IProjectMasterQueryRepository _projectMasterQueryRepository;
//         private readonly IDepartmentGrpcClient _departmentGrpcClient;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly ICurrencyGrpcClient _currencyGrpcClient;
//         private readonly IAssetGroupGrpcClient _assetGroupGrpcClient;
//         private readonly ICostCenterGrpcClient _costCenterGrpcClient;
//         private readonly IFinancialYearGrpcClient _financialYearGrpcClient;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;

//         public GetProjectMasterByIdQueryHandler(IProjectMasterQueryRepository projectMasterQueryRepository, IDepartmentGrpcClient departmentGrpcClient, IUnitGrpcClient unitGrpcClient, IMapper mapper, IMediator mediator
//         , ICurrencyGrpcClient currencyGrpcClient, IAssetGroupGrpcClient assetGroupGrpcClient, ICostCenterGrpcClient costCenterGrpcClient, IFinancialYearGrpcClient financialYearGrpcClient)
//         {
//             _projectMasterQueryRepository = projectMasterQueryRepository;
//             _departmentGrpcClient = departmentGrpcClient;
//             _unitGrpcClient = unitGrpcClient;
//             _mapper = mapper;
//             _mediator = mediator;
//             _currencyGrpcClient = currencyGrpcClient;
//             _assetGroupGrpcClient = assetGroupGrpcClient;
//             _costCenterGrpcClient = costCenterGrpcClient;
//             _financialYearGrpcClient = financialYearGrpcClient;
//         }
        
//           public async Task<GetProjectMasterDto> Handle(
//             GetProjectMasterByIdQuery request,
//             CancellationToken cancellationToken)
//         {
//             // 1️⃣ Get entity from DB
//             var entity = await _projectMasterQueryRepository.GetByIdAsync(request.Id, cancellationToken);

//             if (entity is null)
//             {
//                 // Replace with your custom exception (EntityNotFoundException, etc.)
//                 throw new Exception($"ProjectMaster with Id {request.Id} not found.");
//             }

//             // 2️⃣ Map entity → DTO via AutoMapper
//             var project = _mapper.Map<GetProjectMasterDto>(entity);

//             // 3️⃣ Parallel gRPC calls: Department, Unit, Currency, AssetGroup

//             var deptTask = _departmentGrpcClient.GetAllDepartmentAsync();
//             var unitTask = _unitGrpcClient.GetAllUnitAsync();

//             // Currency – single Id
//             Task<CurrencyDto?> currencyTask = Task.FromResult<CurrencyDto?>(null);
//             if (project.CurrencyId > 0)
//             {
//                 currencyTask = _currencyGrpcClient.GetByIdAsync(project.CurrencyId, cancellationToken);
//             }

//             // AssetGroup – single Id
//             Task<AssetGroupDto?> assetGroupTask = Task.FromResult<AssetGroupDto?>(null);
//             if (project.AssetGroupId > 0)
//             {
//                 assetGroupTask = _assetGroupGrpcClient.GetByIdAsync(project.AssetGroupId, cancellationToken);
//             }

//             await Task.WhenAll(deptTask, unitTask, currencyTask, assetGroupTask);

//             var departments = await deptTask;
//             var units       = await unitTask;
//             var currency    = await currencyTask;
//             var assetGroup  = await assetGroupTask;

//             // 4️⃣ Resolve Department & Unit names
//             var deptMap = departments
//                 .Where(d => d.DepartmentId > 0)
//                 .ToDictionary(d => d.DepartmentId, d => d.DepartmentName ?? string.Empty);

//             var unitMap = units
//                 .Where(u => u.UnitId > 0)
//                 .ToDictionary(u => u.UnitId, u => u.UnitName ?? string.Empty);

//             if (deptMap.TryGetValue(project.DepartmentId, out var deptName) &&
//                 !string.IsNullOrWhiteSpace(deptName))
//             {
//                 project.DepartmentName = deptName;
//             }

//             if (unitMap.TryGetValue(project.UnitId, out var unitName) &&
//                 !string.IsNullOrWhiteSpace(unitName))
//             {
//                 project.UnitName = unitName;
//             }

//             // 5️⃣ Resolve Currency name (from single-id gRPC)
//             if (currency is not null)
//             {
//                 // Prefer Code, fallback to Name
//                 project.CurrencyName = !string.IsNullOrWhiteSpace(currency.Code)
//                     ? currency.Code
//                     : currency.Name;
//             }

//             // 6️⃣ Resolve AssetGroup name (from FixedAsset gRPC)
//             if (assetGroup is not null &&
//                 !string.IsNullOrWhiteSpace(assetGroup.GroupName))
//             {
//                 project.AssetGroup = assetGroup.GroupName;
//             }

//             // 7️⃣ Domain event – Audit log
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode: "GetProjectMasterByIdQuery",
//                 actionName: project.Id.ToString(),
//                 details: $"ProjectMaster details {project.Id} were fetched.",
//                 module: "ProjectMaster"
//             );

//             await _mediator.Publish(domainEvent, cancellationToken);

//             return project;
//         }
        
//         //     public async Task<ProjectMasterDto> Handle(    GetProjectMasterByIdQuery request,   CancellationToken cancellationToken)
//         // {
//         //     // 1️⃣ Get entity from DB
//         //     var entity = await _projectMasterQueryRepository.GetByIdAsync(request.Id, cancellationToken);

//         //     if (entity is null)
//         //     {
//         //         // Replace with your custom exception (EntityNotFoundException, etc.)
//         //         throw new Exception($"ProjectMaster with Id {request.Id} not found.");
//         //     }

//         //     // 2️⃣ Map entity → base DTO via AutoMapper
//         //     var project = _mapper.Map<ProjectMasterDto>(entity);

//         //     // 3️⃣ Parallel gRPC calls: Department, Unit, Currency (single-id)

//         //     // Department & Unit (no ct here if your interface doesn’t support it)
//         //     var deptTask = _departmentGrpcClient.GetAllDepartmentAsync();
//         //     var unitTask = _unitGrpcClient.GetAllUnitAsync();

//         //     // Currency – single Id
//         //     Task<CurrencyDto?> currencyTask = Task.FromResult<CurrencyDto?>(null);
//         //     if (project.CurrencyId > 0)
//         //     {
//         //         currencyTask = _currencyGrpcClient.GetByIdAsync(project.CurrencyId, cancellationToken);
//         //     }
//         //       // AssetGroup – single Id
//         //     Task<AssetGroupDto?> assetGroupTask = Task.FromResult<AssetGroupDto?>(null);
//         //     if (project.AssetGroupId > 0)
//         //     {
//         //         assetGroupTask = _assetGroupGrpcClient.GetByIdAsync(project.AssetGroupId, cancellationToken);
//         //     }

//         //     // Wait for all
//         //     await Task.WhenAll(deptTask, unitTask, currencyTask, assetGroupTask);

//         //     var departments = await deptTask;
//         //     var units       = await unitTask;
//         //     var currency    = await currencyTask;
//         //     var assetGroup  = await assetGroupTask;

//         //     // 4️⃣ Resolve Department & Unit names
//         //     var deptMap = departments
//         //         .Where(d => d.DepartmentId > 0)
//         //         .ToDictionary(d => d.DepartmentId, d => d.DepartmentName ?? string.Empty);

//         //     var unitMap = units
//         //         .Where(u => u.UnitId > 0)
//         //         .ToDictionary(u => u.UnitId, u => u.UnitName ?? string.Empty);

//         //     if (deptMap.TryGetValue(project.DepartmentId, out var deptName) &&
//         //         !string.IsNullOrWhiteSpace(deptName))
//         //     {
//         //         project.DepartmentName = deptName;
//         //     }

//         //     if (unitMap.TryGetValue(project.UnitId, out var unitName) &&
//         //         !string.IsNullOrWhiteSpace(unitName))
//         //     {
//         //         project.UnitName = unitName;
//         //     }

//         //     // 5️⃣ Resolve Currency name (from single-id gRPC)
//         //     if (currency is not null)
//         //     {
//         //         // Prefer Code, fallback to Name
//         //         project.CurrencyName = !string.IsNullOrWhiteSpace(currency.Code)
//         //             ? currency.Code
//         //             : currency.Name;
//         //     }

//         //     if (assetGroup is not null &&
//         //         !string.IsNullOrWhiteSpace(assetGroup.GroupName))
//         //     {
//         //         project.AssetGroup = assetGroup.GroupName;
//         //     }

//         //     // 6️⃣ Domain event – Audit log
//         //     var domainEvent = new AuditLogsDomainEvent(
//         //         actionDetail: "GetById",
//         //         actionCode: "GetProjectMasterByIdQuery",
//         //         actionName: project.Id.ToString(),
//         //         details: $"ProjectMaster details {project.Id} were fetched.",
//         //         module: "ProjectMaster"
//         //     );

//         //     await _mediator.Publish(domainEvent, cancellationToken);

//         //     return project;
//         // }

//         //    public async Task<ProjectMasterDto> Handle(
//         //         GetProjectMasterByIdQuery request,
//         //         CancellationToken cancellationToken)
//         //     {
//         //         // 1. Get entity from DB
//         //         var result = await _projectMasterQueryRepository.GetByIdAsync(request.Id);

//         //         // (Optional: handle not-found with your custom exception)
//         //         // if (result == null)
//         //         //     throw new EntityNotFoundException($"ProjectMaster with Id {request.Id} not found.");

//         //         // 2. Map to DTO
//         //         var project = _mapper.Map<ProjectMasterDto>(result);

//         //         // 3. Enrich with Department / Unit names via gRPC
//         //         var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
//         //         var units = await _unitGrpcClient.GetAllUnitAsync();

//         //         //var currencies = await _currencyGrpcClient.GetAllCurrencyAsync();

//         //         var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//         //         var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

//         //         if (departmentLookup.TryGetValue(project.DepartmentId, out var departmentName) && departmentName != null)
//         //         {
//         //             project.DepartmentName = departmentName;
//         //         }

//         //         if (unitLookup.TryGetValue(project.UnitId, out var unitName) && unitName != null)
//         //         {
//         //             project.UnitName = unitName;
//         //         }

//         //         // 4. Domain event – Audit log
//         //         var domainEvent = new AuditLogsDomainEvent(
//         //             actionDetail: "GetById",
//         //             actionCode: "GetProjectMasterByIdQuery",
//         //             actionName: project.Id.ToString(),
//         //             details: $"ProjectMaster details {project.Id} were fetched.",
//         //             module: "ProjectMaster"
//         //         );

//         //         await _mediator.Publish(domainEvent, cancellationToken);

//         //         return project;
//         //     }
//     }
// }