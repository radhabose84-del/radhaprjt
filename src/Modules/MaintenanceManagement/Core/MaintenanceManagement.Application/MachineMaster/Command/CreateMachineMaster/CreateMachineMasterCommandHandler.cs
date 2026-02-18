#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster
{
    public class CreateMachineMasterCommandHandler : IRequestHandler<CreateMachineMasterCommand, int>
    {
        private readonly IMachineMasterCommandRepository _iMachineMasterCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

         public CreateMachineMasterCommandHandler(IMachineMasterCommandRepository iMachineMasterCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iMachineMasterCommandRepository = iMachineMasterCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreateMachineMasterCommand request, CancellationToken cancellationToken)
        {
            var machineMaster = _imapper.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(request);
            
            var result = await _iMachineMasterCommandRepository.CreateAsync(machineMaster);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: machineMaster.MachineCode,
                actionName: machineMaster.MachineName,
                details: $"MachineMaster details was created",
                module: "MachineMaster");
            await _imediator.Publish(domainEvent, cancellationToken);
         
            return result > 0 ? result : throw new ExceptionRules("MachineMaster Creation Failed.");
        }
    }
}