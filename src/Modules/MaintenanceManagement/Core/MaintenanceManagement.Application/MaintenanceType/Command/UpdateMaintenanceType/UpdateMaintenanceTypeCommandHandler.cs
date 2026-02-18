#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType
{
    public class UpdateMaintenanceTypeCommandHandler : IRequestHandler<UpdateMaintenanceTypeCommand, int>
    {
        private readonly IMaintenanceTypeCommandRepository _iMaintenanceTypeCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdateMaintenanceTypeCommandHandler(IMaintenanceTypeCommandRepository iMaintenanceTypeCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iMaintenanceTypeCommandRepository = iMaintenanceTypeCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(UpdateMaintenanceTypeCommand request, CancellationToken cancellationToken)
        {
            var maintenanceCategory = _imapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceType>(request);
            var result = await _iMaintenanceTypeCommandRepository.UpdateAsync(request.Id, maintenanceCategory);
         
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: maintenanceCategory.Id.ToString(),
                actionName: maintenanceCategory.TypeName,
                details: $"MaintenanceType details was updated",
                module: "MaintenanceType");
            await _imediator.Publish(domainEvent, cancellationToken);

            return result <= 0 ? throw new ExceptionRules("MaintenanceType Update Failed.") : result;
        }
    }
}