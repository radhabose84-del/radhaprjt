using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster
{
    public class UpdateMachineMasterCommandHandler : IRequestHandler<UpdateMachineMasterCommand, bool>
    {
        private readonly IMachineMasterCommandRepository _iMachineMasterCommandRepository;
        private readonly IMachineMasterQueryRepository _machineQueryRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        
        public UpdateMachineMasterCommandHandler(IMachineMasterCommandRepository iMachineMasterCommandRepository, IMachineMasterQueryRepository machineQueryRepository, IMediator imediator, IMapper imapper)
        {
            _iMachineMasterCommandRepository = iMachineMasterCommandRepository;
            _machineQueryRepository = machineQueryRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<bool> Handle(UpdateMachineMasterCommand request, CancellationToken cancellationToken)
        {
             var machineMaster = _imapper.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(request);
            var result = await _iMachineMasterCommandRepository.UpdateAsync(request.Id, machineMaster);

            if (request.IsActive == 0)
            {
                var linked = await _machineQueryRepository.IsMachineLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }
          
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: machineMaster.Id.ToString(),
                actionName: machineMaster.MachineName ?? "NULL",
                details: $"MachineMaster details was updated",
                module: "MachineMaster");
            await _imediator.Publish(domainEvent, cancellationToken);
           
            return result == true ? result : throw new ExceptionRules("MachineMaster Updation Failed.");
        }
    }
}