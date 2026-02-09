// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Dtos.Inventory;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using WarehouseManagement.Application.Common.HttpResponse;
// using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
// using WarehouseManagement.Domain.Common;
// using WarehouseManagement.Domain.Events;
// using MediatR;

// namespace WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster
// {
//     public class GetAllRackMasterQueryHanlder   : IRequestHandler<GetAllRackMasterQuery, ApiResponseDTO<List<RackMasterDto>>>
//     {
//         private readonly IRackMasterQueryRepository _rackMasterQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IUOMGrpcClient _uOMGrpcClient;

//         private readonly IMiscMasterGrpcClient _miscMasterGrpcClient;

//         public GetAllRackMasterQueryHanlder(IRackMasterQueryRepository rackMasterQueryRepository, IMapper mapper, IMediator mediator, IMiscMasterGrpcClient miscMasterGrpcClient, IUOMGrpcClient uOMGrpcClient)
//         {
//             _rackMasterQueryRepository = rackMasterQueryRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _miscMasterGrpcClient = miscMasterGrpcClient;
//             _uOMGrpcClient = uOMGrpcClient;
//         }

        
//         public async Task<ApiResponseDTO<List<RackMasterDto>>> Handle(  GetAllRackMasterQuery request,   CancellationToken cancellationToken)
//         {

                 
//             // Fetch (repo returns (items, totalCount))
//             var (racks, totalCount) = await _rackMasterQueryRepository
//                 .GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

//             // Map
//             var rackList = _mapper.Map<List<RackMasterDto>>(racks);
//             var floorTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync(WarehouseManagement.Domain.Common.MiscEnumEntity.MiscTypes.Floor);
//             var aisleTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync(WarehouseManagement.Domain.Common.MiscEnumEntity.MiscTypes.WarehouseAisle);
//             var levelTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync(WarehouseManagement.Domain.Common.MiscEnumEntity.MiscTypes.WarehouseRackLevel);

//             //  var floorTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync("Floor");
//             // var aisleTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync("WarehouseAisle");
//             // var levelTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync("WarehouseRackLevel");
//             var uomTask   = _uOMGrpcClient.GetUOMAsync(); // <-- you only have this (get all UOMs)

//             await Task.WhenAll(floorTask, aisleTask, levelTask, uomTask);

//             // Null-safe results
//             var floors = floorTask.Result ?? new List<MiscMasterDto>();
//             var aisles = aisleTask.Result ?? new List<MiscMasterDto>();
//             var levels = levelTask.Result ?? new List<MiscMasterDto>();
//             var uoms = uomTask.Result ; // whatever your return type is

//             // Lookups (use Code or Description as you prefer)
//             var floorById = floors.ToDictionary(x => x.Id, x => x.Description);
//             var aisleById = aisles.ToDictionary(x => x.Id, x => x.Description);
//             var levelById = levels.ToDictionary(x => x.Id, x => x.Description);

//             // UOM name: prefer Name; fallback to Code (adjust property names to your DTO)
//             var uomById = uoms.ToDictionary(
//                     u => u.Id,
//                     u => string.IsNullOrWhiteSpace(u.UOMName) ? u.Code : u.UOMName
//                 );

//            // var uomById = uoms.ToDictionary(u => u.Id, pickUomDisplay);

//             // Hydrate DTOs
//             foreach (var dto in rackList)
//             {
//                 if (dto.FloorId.HasValue     && floorById.TryGetValue(dto.FloorId.Value, out var f)) dto.FloorName       = f;
//                 if (dto.AisleId.HasValue     && aisleById.TryGetValue(dto.AisleId.Value, out var a)) dto.AisleName       = a;
//                 if (dto.RackLevelId.HasValue && levelById.TryGetValue(dto.RackLevelId.Value, out var l)) dto.RackLevelName = l;

//                 if (dto.DimensionUOMId.HasValue && uomById.TryGetValue(dto.DimensionUOMId.Value, out var dn)) dto.DimensionUOMName = dn;
//                 if (dto.CapacityUOMId.HasValue  && uomById.TryGetValue(dto.CapacityUOMId.Value,  out var cn)) dto.CapacityUOMName  = cn;
//             }

//             // Domain event (audit)
//             var domainEvent = new AuditLogsDomainEvent(
//                 actionDetail: "GetAll",
//                 actionCode: "",
//                 actionName: "",
//                 details: "RackMaster list fetched.",
//                 module:  "RackMaster"
//             );
//             await _mediator.Publish(domainEvent, cancellationToken);

//             // Response
//             return new ApiResponseDTO<List<RackMasterDto>>
//             {
//                 IsSuccess  = true,
//                 Message    = "Success",
//                 Data       = rackList,
//                 TotalCount = totalCount,
//                 PageNumber = request.PageNumber,
//                 PageSize   = request.PageSize
//             };
//             }
//     }
// }