using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.External.IInvetoryManagement;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;

namespace WarehouseManagement.Application.BinMaster.Queries.GetBinMasterById
{
    public class GetBinMasterByIdQueryHandler : IRequestHandler<GetBinMasterByIdQuery, BinMasterDto>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        private readonly IBinMasterQueryRepository _binMasterQueryRepository;                
        private readonly IWarehouseMasterQueryRepository _warehouseRepo;  // if you have it
        private readonly IRackMasterQueryRepository _rackRepo;            // if you have it
        private readonly IUOMLookup _uomLookup;        
        private readonly IMiscMasterLookup _miscMasterLookup;

        public GetBinMasterByIdQueryHandler(IMapper mapper, IBinMasterQueryRepository binMasterQueryRepository, IMediator mediator, IWarehouseMasterQueryRepository warehouseRepo, IRackMasterQueryRepository rackRepo, IUOMLookup uomLookup
            , IMiscMasterLookup miscMasterLookup)
        {
            _mapper = mapper;
            _binMasterQueryRepository = binMasterQueryRepository;
            _mediator = mediator;
            _warehouseRepo = warehouseRepo;
            _rackRepo = rackRepo;
            _uomLookup = uomLookup;
            _miscMasterLookup = miscMasterLookup;
        }

       public async Task<BinMasterDto> Handle(GetBinMasterByIdQuery request, CancellationToken cancellationToken)
            {
                // 1) Fetch
                var entity = await _binMasterQueryRepository.GetByIdAsync(request.Id);
                if (entity is null)
                    throw new ValidationException($"BinMaster with Id {request.Id} not found.");

                // 2) Map
                var dto = _mapper.Map<BinMasterDto>(entity);

                // 3) Lookups in parallel               
                var uomsTask         = _uomLookup.GetAllAsync();                                // Id, UOMName
                var warehousesTask   = _warehouseRepo.GetwarehouseAsync();                          // list with Id, WarehouseName

                Task<RackMasterDto?> rackTask =
                    (dto.RackId.HasValue && dto.RackId.Value > 0)
                        ? _rackRepo.GetByIdAsync(dto.RackId.Value).ContinueWith<RackMasterDto?>(t => t.Result, cancellationToken)
                        : Task.FromResult<RackMasterDto?>(null);

                await Task.WhenAll( uomsTask, warehousesTask, rackTask);

                // 4) Enrich
              

                var uom = uomsTask.Result.FirstOrDefault(x => x.Id == dto.CapacityUOMId);
                if (uom != null) dto.CapacityUOMName = uom.UOMName;

                var wh  = warehousesTask.Result?.FirstOrDefault(x => x.Id == dto.WarehouseId);
                if (wh != null) dto.WarehouseName = wh.WarehouseName;

                var rk  = rackTask.Result;
                if (rk != null)
                {
                    dto.RackCode = rk.RackCode;
                    dto.RackName = rk.RackName;
                }

                // 5) Audit
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",
                    actionName: "",
                    details: $"BinMaster details {dto.Id} was fetched.",
                    module: "BinMaster"
                );
                await _mediator.Publish(domainEvent, cancellationToken);

                return dto;
            }
    }
}