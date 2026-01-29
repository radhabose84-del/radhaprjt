using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster
{
    public class UpdateShiftMasterCommandHandler : IRequestHandler<UpdateShiftMasterCommand, ApiResponseDTO<bool>>
    {
        private readonly IShiftMasterCommand _shiftMasterCommand;
        private readonly IShiftMasterQuery _shiftMasterQueryRepo;
         private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public UpdateShiftMasterCommandHandler(IShiftMasterCommand shiftMasterCommand, IShiftMasterQuery shiftMasterQueryRepo, IMapper mapper, IMediator mediator)
        {
            _shiftMasterCommand = shiftMasterCommand;
            _shiftMasterQueryRepo = shiftMasterQueryRepo;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(UpdateShiftMasterCommand request, CancellationToken cancellationToken)
        {
             var shiftMaster  = _mapper.Map<MaintenanceManagement.Domain.Entities.ShiftMaster>(request);

            if (request.IsActive == 0)
            {
                var linked = await _shiftMasterQueryRepo.IsShiftMasterLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }       
                var shiftMasterresult = await _shiftMasterCommand.UpdateAsync(shiftMaster);

                
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: "update",
                        actionName: "Update ShiftMaster",
                        details: $"Update ShiftMaster",
                        module:"ShiftMaster"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(shiftMasterresult)
                {
                    return new ApiResponseDTO<bool>
                    {
                        IsSuccess = true, 
                        Message = "ShiftMaster updated successfully."
                    };
                }

                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false, 
                    Message = "ShiftMaster not updated."
                };
        }
    }
}