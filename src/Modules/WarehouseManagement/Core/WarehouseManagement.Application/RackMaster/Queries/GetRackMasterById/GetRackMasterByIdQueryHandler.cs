using System.ComponentModel.DataAnnotations;
using AutoMapper;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Dtos.Lookups.Inventory;

namespace WarehouseManagement.Application.RackMaster.Queries.GetRackMasterById
{
    public class GetRackMasterByIdQueryHandler : IRequestHandler<GetRackMasterByIdQuery, RackMasterDto>
    {

        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        private readonly IRackMasterQueryRepository _rackMasterQueryRepository;
        private readonly IUOMLookup _uomLookup;

        private readonly IMiscMasterLookup _miscMasterLookup;


        public GetRackMasterByIdQueryHandler(IMapper mapper, IRackMasterQueryRepository rackMasterQueryRepository, IMediator mediator, IMiscMasterLookup miscMasterLookup, IUOMLookup uomLookup)
        {
            _mapper = mapper;
            _rackMasterQueryRepository = rackMasterQueryRepository;
            _mediator = mediator;
            _miscMasterLookup = miscMasterLookup;
            _uomLookup = uomLookup;
        }
        public async Task<RackMasterDto> Handle(GetRackMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var row = await _rackMasterQueryRepository.GetByIdAsync(request.Id);
            if (row is null) throw new ValidationException($"RackMaster with Id {request.Id} not found.");

            var dto = _mapper.Map<RackMasterDto>(row); // has IDs populated

            var uomTask = _uomLookup.GetAllAsync();

            // Avoid running multiple MiscMaster lookups concurrently on the same repository/connection.
            var floors = await _miscMasterLookup.GetMiscMasterByIdAsync(Domain.Common.MiscEnumEntity.MiscTypes.Floor)
                         ?? new List<MiscMasterLookupDto>();
            var aisles = await _miscMasterLookup.GetMiscMasterByIdAsync(Domain.Common.MiscEnumEntity.MiscTypes.WarehouseAisle)
                         ?? new List<MiscMasterLookupDto>();
            var levels = await _miscMasterLookup.GetMiscMasterByIdAsync(Domain.Common.MiscEnumEntity.MiscTypes.WarehouseRackLevel)
                         ?? new List<MiscMasterLookupDto>();
            var uoms = await uomTask ?? new List<UOMLookupDto>(); // adjust type name

            var floorById = floors.ToDictionary(x => x.Id, x => x.Description);
            var aisleById = aisles.ToDictionary(x => x.Id, x => x.Description);
            var levelById = levels.ToDictionary(x => x.Id, x => x.Description);
            var uomById   = uoms.ToDictionary(u => u.Id, u => string.IsNullOrWhiteSpace(u.UOMName) ? u.Code : u.UOMName);

            if (dto.FloorId.HasValue     && floorById.TryGetValue(dto.FloorId.Value, out var f)) dto.FloorName       = f;
            if (dto.AisleId.HasValue     && aisleById.TryGetValue(dto.AisleId.Value, out var a)) dto.AisleName       = a;
            if (dto.RackLevelId.HasValue && levelById.TryGetValue(dto.RackLevelId.Value, out var l)) dto.RackLevelName = l;

            if (dto.DimensionUOMId.HasValue && uomById.TryGetValue(dto.DimensionUOMId.Value, out var dn)) dto.DimensionUOMName = dn;
            if (dto.CapacityUOMId.HasValue  && uomById.TryGetValue(dto.CapacityUOMId.Value,  out var cn)) dto.CapacityUOMName  = cn;

            // audit
            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode:   "",
                actionName:   "",
                details:      $"RackMaster details {dto.Id} was fetched.",
                module:       "RackMaster"), cancellationToken);

            return dto;
        }


        // public async Task<RackMasterDto> Handle(GetRackMasterByIdQuery request, CancellationToken cancellationToken)
        // {

        //     var result = await _rackMasterQueryRepository.GetByIdAsync(request.Id);
        //     if (result is null )
        //     {
        //         throw new ValidationException($"RackMaster with Id {request.Id} not found.");

        //     }

        //     var rackMasterDto = _mapper.Map<RackMasterDto>(result);

        //     //Domain Event
        //             var domainEvent = new AuditLogsDomainEvent(
        //                 actionDetail: "GetById",
        //                 actionCode: "",        
        //                 actionName: "",
        //                 details: $"  RackMaster details {rackMasterDto.Id} was fetched.",
        //                 module:"RackMaster"
        //             );
        //             await _mediator.Publish(domainEvent, cancellationToken);
        //     return  rackMasterDto;
        // }

    }
    
}
