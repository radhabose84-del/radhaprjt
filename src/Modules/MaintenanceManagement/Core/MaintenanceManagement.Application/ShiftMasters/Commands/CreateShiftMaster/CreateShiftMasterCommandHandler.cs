using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster
{
    public class CreateShiftMasterCommandHandler : IRequestHandler<CreateShiftMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IShiftMasterCommand _shiftMasterCommand;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateShiftMasterCommandHandler(IShiftMasterCommand shiftMasterCommand, IMediator mediator, IMapper mapper)
        {
            _shiftMasterCommand = shiftMasterCommand;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<ApiResponseDTO<int>> Handle(CreateShiftMasterCommand request, CancellationToken cancellationToken)
        {
            var shiftMaster  = _mapper.Map<MaintenanceManagement.Domain.Entities.ShiftMaster>(request);

                var shiftMasterresult = await _shiftMasterCommand.CreateAsync(shiftMaster);
                
                
                if (shiftMasterresult > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: "NewData",
                     actionName: "Shift Master Creation",
                     details: $"Shift Master Creation",
                     module:"Shift Master"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = true, 
                        Message = "Shift Master successfully", 
                        Data = shiftMasterresult
                    };
                }
               
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false, 
                        Message = "Shift Master not created"
                    };
        }
    }
}