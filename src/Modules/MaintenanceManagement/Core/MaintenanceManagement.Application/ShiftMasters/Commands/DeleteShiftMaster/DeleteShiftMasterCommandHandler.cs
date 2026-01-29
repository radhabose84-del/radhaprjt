using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster
{
    public class DeleteShiftMasterCommandHandler : IRequestHandler<DeleteShiftMasterCommand, ApiResponseDTO<bool>>
    {
        private readonly IShiftMasterCommand _shiftMasterCommand;
        private readonly IShiftMasterQuery _shiftMasterQueryRepo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public DeleteShiftMasterCommandHandler(IShiftMasterCommand shiftMasterCommand, IShiftMasterQuery shiftMasterQueryRepo, IMapper mapper, IMediator mediator)
        {
            _shiftMasterCommand = shiftMasterCommand;
            _shiftMasterQueryRepo = shiftMasterQueryRepo;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<bool>> Handle(DeleteShiftMasterCommand request, CancellationToken cancellationToken)
        {
              var shiftMaster  = _mapper.Map<MaintenanceManagement.Domain.Entities.ShiftMaster>(request);


        var linked = await _shiftMasterQueryRepo.IsShiftMasterLinkedAsync(request.Id);
            if (linked)
            {
         throw new ValidationException("This master is linked with other records. You cannot delete this record.");
           }

            var shiftMasterresult = await _shiftMasterCommand.DeleteAsync(request.Id,shiftMaster);


                  //Domain Event  
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: "delete",
                        actionName: "Delete Shift Master",
                        details: $"Delete Shift Master",
                        module:"Shift Master"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken);  

                 if(shiftMasterresult)
                {
                    return new ApiResponseDTO<bool>
                    {
                        IsSuccess = true, 
                        Message = "Shift Master deleted successfully."
                    };
                }

                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false, 
                    Message = "Shift Master not deleted."
                };
        }
    }
}