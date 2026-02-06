// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using WarehouseManagement.Application.Common.HttpResponse;
// using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
// using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
// using Contracts.Interfaces.External.IInvetoryManagement;
// using Contracts.Dtos.Inventory;
// using WarehouseManagement.Domain.Events;
// using MediatR;

// namespace WarehouseManagement.Application.RackMaster.Queries.GetRackMasterById
// {
//     public class GetRackMasterByIdQueryHandler : IRequestHandler<GetRackMasterByIdQuery, RackMasterDto>
//     {

//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;

//         private readonly IRackMasterQueryRepository _rackMasterQueryRepository;
//         private readonly IUOMGrpcClient _uOMGrpcClient;

//         private readonly IMiscMasterGrpcClient _miscMasterGrpcClient;


//         public GetRackMasterByIdQueryHandler(IMapper mapper, IRackMasterQueryRepository rackMasterQueryRepository, IMediator mediator, IUOMGrpcClient uOMGrpcClient, IMiscMasterGrpcClient miscMasterGrpcClient)
//         {
//             _mapper = mapper;
//             _rackMasterQueryRepository = rackMasterQueryRepository;
//             _mediator = mediator;
//             _uOMGrpcClient = uOMGrpcClient;
//             _miscMasterGrpcClient = miscMasterGrpcClient;
//         }
//         public async Task<RackMasterDto> Handle(GetRackMasterByIdQuery request, CancellationToken cancellationToken)
//         {
//             var row = await _rackMasterQueryRepository.GetByIdAsync(request.Id);
//             if (row is null) throw new ValidationException($"RackMaster with Id {request.Id} not found.");

//             var dto = _mapper.Map<RackMasterDto>(row); // has IDs populated

//             // fetch lookups in parallel
//             var floorTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync(Domain.Common.MiscEnumEntity.MiscTypes.Floor);
//             var aisleTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync(Domain.Common.MiscEnumEntity.MiscTypes.WarehouseAisle);
//             var levelTask = _miscMasterGrpcClient.GetMiscMasterByIdAsync(Domain.Common.MiscEnumEntity.MiscTypes.WarehouseRackLevel);
//             var uomTask   = _uOMGrpcClient.GetUOMAsync();

//             await Task.WhenAll(floorTask, aisleTask, levelTask, uomTask);

//             var floors = floorTask.Result ?? new List<MiscMasterDto>();
//             var aisles = aisleTask.Result ?? new List<MiscMasterDto>();
//             var levels = levelTask.Result ?? new List<MiscMasterDto>();
//             var uoms   = uomTask.Result   ?? new List<UOMMasterDto>(); // adjust type name

//             var floorById = floors.ToDictionary(x => x.Id, x => x.Description);
//             var aisleById = aisles.ToDictionary(x => x.Id, x => x.Description);
//             var levelById = levels.ToDictionary(x => x.Id, x => x.Description);
//             var uomById   = uoms.ToDictionary(u => u.Id, u => string.IsNullOrWhiteSpace(u.UOMName) ? u.Code : u.UOMName);

//             if (dto.FloorId.HasValue     && floorById.TryGetValue(dto.FloorId.Value, out var f)) dto.FloorName       = f;
//             if (dto.AisleId.HasValue     && aisleById.TryGetValue(dto.AisleId.Value, out var a)) dto.AisleName       = a;
//             if (dto.RackLevelId.HasValue && levelById.TryGetValue(dto.RackLevelId.Value, out var l)) dto.RackLevelName = l;

//             if (dto.DimensionUOMId.HasValue && uomById.TryGetValue(dto.DimensionUOMId.Value, out var dn)) dto.DimensionUOMName = dn;
//             if (dto.CapacityUOMId.HasValue  && uomById.TryGetValue(dto.CapacityUOMId.Value,  out var cn)) dto.CapacityUOMName  = cn;

//             // audit
//             await _mediator.Publish(new AuditLogsDomainEvent(
//                 actionDetail: "GetById",
//                 actionCode:   "",
//                 actionName:   "",
//                 details:      $"RackMaster details {dto.Id} was fetched.",
//                 module:       "RackMaster"), cancellationToken);

//             return dto;
//         }


//         // public async Task<RackMasterDto> Handle(GetRackMasterByIdQuery request, CancellationToken cancellationToken)
//         // {

//         //     var result = await _rackMasterQueryRepository.GetByIdAsync(request.Id);
//         //     if (result is null )
//         //     {
//         //         throw new ValidationException($"RackMaster with Id {request.Id} not found.");

//         //     }

//         //     var rackMasterDto = _mapper.Map<RackMasterDto>(result);

//         //     //Domain Event
//         //             var domainEvent = new AuditLogsDomainEvent(
//         //                 actionDetail: "GetById",
//         //                 actionCode: "",        
//         //                 actionName: "",
//         //                 details: $"  RackMaster details {rackMasterDto.Id} was fetched.",
//         //                 module:"RackMaster"
//         //             );
//         //             await _mediator.Publish(domainEvent, cancellationToken);
//         //     return  rackMasterDto;
//         // }

//     }
    
// }