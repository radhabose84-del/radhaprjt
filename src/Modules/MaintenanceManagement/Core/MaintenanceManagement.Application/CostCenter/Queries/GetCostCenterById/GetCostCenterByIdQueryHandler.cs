// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
// using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterById
// {
//     public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, CostCenterDto>
//     {

//         private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IDepartmentGrpcClient _departmentGrpcClient;
//         private readonly IUnitGrpcClient _unitGrpcClient;


//         public GetCostCenterByIdQueryHandler(ICostCenterQueryRepository iCostCenterQueryRepository, IMapper mapper, IMediator mediator, IDepartmentGrpcClient departmentService, IUnitGrpcClient unitGrpcClient)
//         {
//             _iCostCenterQueryRepository = iCostCenterQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _departmentGrpcClient = departmentService;
//             _unitGrpcClient = unitGrpcClient;
//         }

//         public async Task<CostCenterDto> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
//         {
//             var result = await _iCostCenterQueryRepository.GetByIdAsync(request.Id);
           
//             var costCenter = _mapper.Map<CostCenterDto>(result);


//             var departments = await _departmentGrpcClient.GetAllDepartmentAsync();
//             var units = await _unitGrpcClient.GetAllUnitAsync();
//             var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
//             var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

//             if ((departmentLookup.TryGetValue(costCenter.DepartmentId, out var departmentName) && departmentName != null) |
//                  (unitLookup.TryGetValue(costCenter.UnitId, out var unitName) && unitName != null))
//             {
//                 costCenter.DepartmentName = departmentName;
//                 costCenter.UnitName = unitName;
//             }

//             //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode: "GetCostCenterByIdQuery",
//                 actionName: costCenter.Id.ToString(),
//                 details: $"CostCenter details {costCenter.Id} was fetched.",
//                 module: "CostCenter"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);
//             return costCenter;
//         }

//     }
// }