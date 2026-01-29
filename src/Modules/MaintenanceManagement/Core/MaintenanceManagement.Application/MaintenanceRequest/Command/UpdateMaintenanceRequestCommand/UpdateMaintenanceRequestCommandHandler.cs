using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
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

         public UpdateMaintenanceRequestCommandHandler( IMaintenanceRequestCommandRepository repository , IMapper mapper , IMediator mediator )
         {
            _maintenanceRequestCommandRepository = repository;
            _mapper = mapper;
            _mediator = mediator;
         }

         public async Task<ApiResponseDTO<bool>> Handle(UpdateMaintenanceRequestCommand request, CancellationToken cancellationToken)
        {
           // Map the command to the domain entity
            var maintenanceRequestEntity = _mapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceRequest>(request);

            // Attempt update
            var updateResult = await _maintenanceRequestCommandRepository.UpdateAsync(maintenanceRequestEntity);

           
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update Maintenance Request",
                actionCode: "UPDATE",
                actionName: "Update MaintenanceRequest",
                details: $"MaintenanceRequest updated. RequestId: {request.Id}",
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