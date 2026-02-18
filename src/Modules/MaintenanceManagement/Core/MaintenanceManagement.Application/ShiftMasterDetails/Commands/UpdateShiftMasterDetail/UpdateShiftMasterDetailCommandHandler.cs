using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail
{
    public class UpdateShiftMasterDetailCommandHandler : IRequestHandler<UpdateShiftMasterDetailCommand, ApiResponseDTO<bool>>
    {
        private readonly IShiftMasterDetailCommand _shiftMasterDetailCommand;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public UpdateShiftMasterDetailCommandHandler(IShiftMasterDetailCommand shiftMasterDetailCommand, IMapper mapper, IMediator mediator)
        {
            _shiftMasterDetailCommand = shiftMasterDetailCommand;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<bool>> Handle(UpdateShiftMasterDetailCommand request, CancellationToken cancellationToken)
        {
            var shiftMaster  = _mapper.Map<ShiftMasterDetail>(request);
         
                var shiftMasterresult = await _shiftMasterDetailCommand.UpdateAsync(shiftMaster);

                
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: "update",
                        actionName: "Update ShiftMaster Detail",
                        details: $"Update ShiftMaster Detail",
                        module:"ShiftMaster Detail"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(shiftMasterresult)
                {
                    return new ApiResponseDTO<bool>
                    {
                        IsSuccess = true, 
                        Message = "ShiftMaster detail updated successfully."
                    };
                }

                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false, 
                    Message = "ShiftMaster detail not updated."
                };
        }
    }
}