#nullable disable
using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Common;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster
{
    public class GetAllRackMasterQueryHanlder   : IRequestHandler<GetAllRackMasterQuery, ApiResponseDTO<List<RackMasterDto>>>
    {
        private readonly IRackMasterQueryRepository _rackMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUOMLookup _uomLookup;

        private readonly IMiscMasterLookup _miscMasterLookup;

        public GetAllRackMasterQueryHanlder(IRackMasterQueryRepository rackMasterQueryRepository, IMapper mapper, IMediator mediator, IMiscMasterLookup miscMasterLookup, IUOMLookup uomLookup)
        {
            _rackMasterQueryRepository = rackMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _miscMasterLookup = miscMasterLookup;
            _uomLookup = uomLookup;
        }

        
        public async Task<ApiResponseDTO<List<RackMasterDto>>> Handle(  GetAllRackMasterQuery request,   CancellationToken cancellationToken)
        {

                 
            // Fetch (repo returns (items, totalCount))
            var (racks, totalCount) = await _rackMasterQueryRepository
                .GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            // Map
            var rackList = _mapper.Map<List<RackMasterDto>>(racks);

            var uomTask = _uomLookup.GetAllAsync();

            // Avoid running multiple MiscMaster lookups concurrently on the same repository/connection.
            var floors = await _miscMasterLookup.GetMiscMasterByIdAsync(WarehouseManagement.Domain.Common.MiscEnumEntity.MiscTypes.Floor)
                         ?? new List<MiscMasterLookupDto>();
            var aisles = await _miscMasterLookup.GetMiscMasterByIdAsync(WarehouseManagement.Domain.Common.MiscEnumEntity.MiscTypes.WarehouseAisle)
                         ?? new List<MiscMasterLookupDto>();
            var levels = await _miscMasterLookup.GetMiscMasterByIdAsync(WarehouseManagement.Domain.Common.MiscEnumEntity.MiscTypes.WarehouseRackLevel)
                         ?? new List<MiscMasterLookupDto>();
            var uoms = await uomTask ?? new List<UOMLookupDto>();

            // Lookups (use Code or Description as you prefer)
            var floorById = floors.ToDictionary(x => x.Id, x => x.Description);
            var aisleById = aisles.ToDictionary(x => x.Id, x => x.Description);
            var levelById = levels.ToDictionary(x => x.Id, x => x.Description);

            // UOM name: prefer Name; fallback to Code (adjust property names to your DTO)
            var uomById = uoms.ToDictionary(
                    u => u.Id,
                    u => string.IsNullOrWhiteSpace(u.UOMName) ? u.Code : u.UOMName
                );

           // var uomById = uoms.ToDictionary(u => u.Id, pickUomDisplay);

            // Hydrate DTOs
            foreach (var dto in rackList)
            {
                if (dto.FloorId.HasValue     && floorById.TryGetValue(dto.FloorId.Value, out var f)) dto.FloorName       = f;
                if (dto.AisleId.HasValue     && aisleById.TryGetValue(dto.AisleId.Value, out var a)) dto.AisleName       = a;
                if (dto.RackLevelId.HasValue && levelById.TryGetValue(dto.RackLevelId.Value, out var l)) dto.RackLevelName = l;

                if (dto.DimensionUOMId.HasValue && uomById.TryGetValue(dto.DimensionUOMId.Value, out var dn)) dto.DimensionUOMName = dn;
                if (dto.CapacityUOMId.HasValue  && uomById.TryGetValue(dto.CapacityUOMId.Value,  out var cn)) dto.CapacityUOMName  = cn;
            }

            // Domain event (audit)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "RackMaster list fetched.",
                module:  "RackMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // Response
            return new ApiResponseDTO<List<RackMasterDto>>
            {
                IsSuccess  = true,
                Message    = "Success",
                Data       = rackList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize   = request.PageSize
            };
            }
    }
}
