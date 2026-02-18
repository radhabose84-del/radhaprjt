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

namespace MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail
{
    public class CreateShiftMasterDetailCommandHandler : IRequestHandler<CreateShiftMasterDetailCommand, ApiResponseDTO<int>>
    {
        private readonly IShiftMasterDetailCommand _shiftMasterDetailCommand;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateShiftMasterDetailCommandHandler(IShiftMasterDetailCommand shiftMasterDetailCommand, IMediator mediator, IMapper mapper)
        {
            _shiftMasterDetailCommand = shiftMasterDetailCommand;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<ApiResponseDTO<int>> Handle(CreateShiftMasterDetailCommand request, CancellationToken cancellationToken)
        {
            var shiftMaster  = _mapper.Map<ShiftMasterDetail>(request);

                var shiftMasterresult = await _shiftMasterDetailCommand.CreateAsync(shiftMaster);
                
                
                if (shiftMasterresult > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: "NewData",
                     actionName: "Shift Master detail Creation",
                     details: $"Shift Master detail Creation",
                     module:"Shift Master detail"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = true, 
                        Message = "Shift Master detail successfully", 
                        Data = shiftMasterresult
                    };
                }
               
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false, 
                        Message = "Shift Master detail not created"
                    };
        }
    }
}