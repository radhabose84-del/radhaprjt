using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using WarehouseManagement.Application.Common.HttpResponse;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Events;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster
{
    public class UpdateWarehouseMasterCommandHandler : IRequestHandler<UpdateWarehouseMasterCommand, ApiResponseDTO<bool>>
    {

        private readonly IWarehouseMasterCommandRepository _warehouseMasterCommandRepository;
        private readonly IWarehouseMasterQueryRepository _warehouseMasterQueryRepository;
        private readonly IWarehouseCodeGenerator _warehouseCodeGenerator;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
         private readonly ILocationLookup _locationLookup;


        public UpdateWarehouseMasterCommandHandler(IWarehouseMasterCommandRepository warehouseMasterCommandRepository, IWarehouseMasterQueryRepository warehouseMasterQueryRepository, IWarehouseCodeGenerator warehouseCodeGenerator, IMapper mapper, IMediator mediator, ILocationLookup locationLookup)
        {
            _warehouseMasterCommandRepository = warehouseMasterCommandRepository;
            _warehouseMasterQueryRepository = warehouseMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _warehouseCodeGenerator = warehouseCodeGenerator;
            _locationLookup = locationLookup;
        }
        
       public async Task<ApiResponseDTO<bool>> Handle(UpdateWarehouseMasterCommand request, CancellationToken ct)
        {
            var warehouse = await _warehouseMasterCommandRepository.GetByIdAsync(request.Id);
           //  var warehouse = await _warehouseMasterQueryRepository.GetByIdAsync(request.Id);

            if (warehouse == null)
                return new ApiResponseDTO<bool> { IsSuccess = false, Message = "WarehouseMaster not found." };

            // Keep originals
            var originalUnitId = warehouse.UnitId;
            var originalTypeId = warehouse.WarehouseTypeId;
            var originalCode   = warehouse.WarehouseCode;


              // ---------- LOCATION: Names win; else apply provided IDs; else keep existing ----------
            var hasNames = !string.IsNullOrWhiteSpace(request.City)
                        && !string.IsNullOrWhiteSpace(request.State)
                        && !string.IsNullOrWhiteSpace(request.Country);

            if (hasNames)
            {
                var ids = await _locationLookup.GetLocationAsync(
                    request.City!.Trim(), request.State!.Trim(), request.Country!.Trim(), ct);

                if (ids is null)
                    return new ApiResponseDTO<bool> { IsSuccess = false, Message = "Unable to resolve City/State/Country via lookup." };

                warehouse.CityId    = ids.CityId;
                warehouse.StateId   = ids.StateId;
                warehouse.CountryId = ids.CountryId;
            }
            else
            {
                if (request.CityId    > 0) warehouse.CityId    = request.CityId;
                if (request.StateId   > 0) warehouse.StateId   = request.StateId;
                if (request.CountryId > 0) warehouse.CountryId = request.CountryId;
                // if none provided, we leave existing IDs as-is
            }


            // Map request -> entity (make sure your mapper doesn't overwrite with 0s; see profile below)
            _mapper.Map(request, warehouse);


            // Compute effective values (0 means "no change" from client)
            var effectiveUnitId = request.UnitId == 0 ? originalUnitId : warehouse.UnitId;
            var effectiveTypeId = request.WarehouseTypeId == 0 ? originalTypeId : warehouse.WarehouseTypeId;

            var unitChanged = effectiveUnitId != originalUnitId;
            var typeChanged = effectiveTypeId != originalTypeId;
            var inputsChanged = unitChanged || typeChanged;

            // Ensure entity has the effective values
            warehouse.UnitId = effectiveUnitId;
            warehouse.WarehouseTypeId = effectiveTypeId;

            // Only (re)generate WarehouseCode when required
            if (string.IsNullOrWhiteSpace(warehouse.WarehouseCode))
            {
                warehouse.WarehouseCode = await _warehouseCodeGenerator.GenerateAsync(
                    warehouse.UnitId, warehouse.WarehouseTypeId);
            }
            else if (inputsChanged)
            {
                // Prefer preserving suffix on update if you added this method:
                warehouse.WarehouseCode = await _warehouseCodeGenerator.RebuildForUpdateAsync(
                    warehouse.UnitId, warehouse.WarehouseTypeId, originalCode, warehouse.Id);

               
            }
            // else: keep existing code

            // Only replace mappings if caller sent them
            if (request.AllowedItemGroupIds != null)
            {
                warehouse.AllowedItemGroups.Clear();
                foreach (var itemGroupId in request.AllowedItemGroupIds.Distinct())
                {
                    warehouse.AllowedItemGroups.Add(new WarehouseItemGroupMapping
                    {
                        ItemGroupId = itemGroupId,
                        IsActive = WarehouseManagement.Domain.Common.BaseEntity.Status.Active,
                        IsDeleted = WarehouseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
                    });
                }
            }

            await _warehouseMasterCommandRepository.UpdateAsync(warehouse);

            var audit = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "WAREHOUSE_UPDATE",
                actionName: warehouse.WarehouseName,
                details: $"WarehouseMaster '{warehouse.WarehouseName}' updated. Code: {(originalCode ?? "<none>")} -> {warehouse.WarehouseCode}",
                module: "WarehouseMaster");
            await _mediator.Publish(audit, ct);

            return new ApiResponseDTO<bool> { IsSuccess = true, Message = "WarehouseMaster updated successfully." };
        }



       
    }
}
