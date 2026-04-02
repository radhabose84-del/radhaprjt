using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster
{
    public class DeleteMachineMasterCommandHandler : IRequestHandler<DeleteMachineMasterCommand, bool>
    {
        
        private readonly IMachineMasterCommandRepository _iMachineMasterCommandRepository;
        private readonly IMachineMasterQueryRepository _machineQueryRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
          public DeleteMachineMasterCommandHandler(IMachineMasterCommandRepository iMachineMasterCommandRepository, IMachineMasterQueryRepository machineQueryRepository, IMediator imediator, IMapper imapper)
        {
            _iMachineMasterCommandRepository = iMachineMasterCommandRepository;
            _machineQueryRepository = machineQueryRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<bool> Handle(DeleteMachineMasterCommand request, CancellationToken cancellationToken)
        {
            var machineMaster = _imapper.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(request);
            var result = await _iMachineMasterCommandRepository.DeleteAsync(request.Id,machineMaster);
    
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: machineMaster.Id.ToString(),
                actionName: machineMaster.MachineCode ?? "NULL",
                details: $"MachineMaster details was deleted",
                module: "MachineMaster");
            await _imediator.Publish(domainEvent);

            return result == true ? result : throw new ExceptionRules("MachineMaster deletion failed.");
        }
    }
}