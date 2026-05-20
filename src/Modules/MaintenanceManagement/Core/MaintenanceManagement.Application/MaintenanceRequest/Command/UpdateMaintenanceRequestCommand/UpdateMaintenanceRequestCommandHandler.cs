using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Party;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand
{
    public class UpdateMaintenanceRequestCommandHandler : IRequestHandler<UpdateMaintenanceRequestCommand, ApiResponseDTO<bool>>
    {
         private readonly IMaintenanceRequestCommandRepository _maintenanceRequestCommandRepository;


        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
        private readonly ISupplierLookup _supplierLookup;

         public UpdateMaintenanceRequestCommandHandler( IMaintenanceRequestCommandRepository repository , IMapper mapper , IMediator mediator , IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository , ISupplierLookup supplierLookup )
         {
            _maintenanceRequestCommandRepository = repository;
            _mapper = mapper;
            _mediator = mediator;
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _supplierLookup = supplierLookup;
         }

         public async Task<ApiResponseDTO<bool>> Handle(UpdateMaintenanceRequestCommand request, CancellationToken cancellationToken)
        {
           // Map the command to the domain entity
            var maintenanceRequestEntity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceRequest>(request);

            // ── Vendor (External Service Request) ─────────────────────────────
            // Vendor is derived only from the ERP Party Master. Legacy fetch is
            // removed (OldVendor* never populated); VendorName is resolved
            // server-side from VendorId so it always matches Party Master.
            maintenanceRequestEntity.OldVendorId = null;
            maintenanceRequestEntity.OldVendorName = null;

            var externalTypes = await _maintenanceRequestQueryRepository.GetMaintenanceExternalRequestTypeAsync();
            var externalTypeId = externalTypes?.FirstOrDefault()?.Id;
            var isExternalRequest = externalTypeId.HasValue
                && maintenanceRequestEntity.RequestTypeId == externalTypeId.Value;

            if (isExternalRequest)
            {
                if (!maintenanceRequestEntity.VendorId.HasValue || maintenanceRequestEntity.VendorId.Value <= 0)
                {
                    return new ApiResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message = "Please select a vendor.",
                        Data = false
                    };
                }

                var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(
                    maintenanceRequestEntity.VendorId.Value, cancellationToken);

                if (supplier == null)
                {
                    return new ApiResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message = "Selected vendor is not a valid active supplier.",
                        Data = false
                    };
                }

                maintenanceRequestEntity.VendorId = supplier.Id;
                maintenanceRequestEntity.VendorName = supplier.VendorName;
            }
            else
            {
                maintenanceRequestEntity.VendorId = null;
                maintenanceRequestEntity.VendorName = null;
            }

            // Attempt update
            var updateResult = await _maintenanceRequestCommandRepository.UpdateAsync(maintenanceRequestEntity);

           
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update Maintenance Request",
                actionCode: "UPDATE",
                actionName: "Update MaintenanceRequest",
                details: $"MaintenanceRequest updated. RequestId: {request.Id}. " +
                         $"VendorId: {maintenanceRequestEntity.VendorId?.ToString() ?? "N/A"}, " +
                         $"VendorName: {maintenanceRequestEntity.VendorName ?? "N/A"}",
                module: "MaintenanceRequest"
            );

            await _mediator.Publish(auditEvent, cancellationToken);

            
            if (updateResult)
            {
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "Maintenance request updated successfully.",
                    Data = true
                };
            }

            return new ApiResponseDTO<bool>
            {
                IsSuccess = false,
                Message = "Failed to update maintenance request.",
                Data = false
            };
        }
    }
}