// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using MaintenanceManagement.Application.Common.HttpResponse;
// using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
// using MaintenanceManagement.Domain.Events;
// using MediatR;

// namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption
// {
//     public class GetPowerConsumptionQueryHandler : IRequestHandler<GetPowerConsumptionQuery, ApiResponseDTO<List<GetPowerConsumptionDto>>>
//     {

//         private readonly IPowerConsumptionQueryRepository _powerConsumptionQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IUnitGrpcClient _unitGrpcClient; // 👈 gRPC Inject here

//         public GetPowerConsumptionQueryHandler(IPowerConsumptionQueryRepository powerConsumptionQueryRepository, IMapper mapper, IMediator mediator, IUnitGrpcClient unitGrpcClient)
//         {
//             _powerConsumptionQueryRepository = powerConsumptionQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _unitGrpcClient = unitGrpcClient;
//         }

//         public async Task<ApiResponseDTO<List<GetPowerConsumptionDto>>> Handle(GetPowerConsumptionQuery request, CancellationToken cancellationToken)
//         {
//             var (PowerConsumption, totalCount) = await _powerConsumptionQueryRepository.GetAllPowerConsumptionAsync(request.PageNumber, request.PageSize, request.SearchTerm);            
//             var powerConsumptionGroupgrouplist = _mapper.Map<List<GetPowerConsumptionDto>>(PowerConsumption);

//             var units = await _unitGrpcClient.GetAllUnitAsync();
//             var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);  
//             var powerConsumptionDictionary = new Dictionary<int, GetPowerConsumptionDto>();

//             // 🔥 Map unit names with DataControl to costCenters
//             foreach (var data in powerConsumptionGroupgrouplist)
//             {
//                 if (unitLookup.TryGetValue(data.UnitId, out var unitName) && unitName != null)
//                 {
//                     data.UnitName = unitName;
//                 }

//                 powerConsumptionDictionary[data.UnitId] = data;

//             }         

//             //Domain Event
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetPowerConsumptionQuery",
//                 actionCode: "Get",        
//                 actionName: PowerConsumption.Count().ToString(),
//                 details: $"FeederGroup details was fetched",
//                 module:"FeederGroup"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);
//             return new ApiResponseDTO<List<GetPowerConsumptionDto>> 
//             { 
//                 IsSuccess = true, 
//                 Message = "Success", 
//                 Data = powerConsumptionGroupgrouplist ,
//                 TotalCount = totalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize = request.PageSize
//             };
            
//         }
//     }
// }