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

namespace MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType
{
    public class DeleteMaintenanceTypeCommandHandler : IRequestHandler<DeleteMaintenanceTypeCommand, int>
    {
        private readonly IMaintenanceTypeCommandRepository _iMaintenanceTypeCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public DeleteMaintenanceTypeCommandHandler(IMaintenanceTypeCommandRepository iMaintenanceTypeCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iMaintenanceTypeCommandRepository = iMaintenanceTypeCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(DeleteMaintenanceTypeCommand request, CancellationToken cancellationToken)
        {
            var maintenanceCategory = _imapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceType>(request);
            var result = await _iMaintenanceTypeCommandRepository.DeleteAsync(request.Id,maintenanceCategory);
          

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: maintenanceCategory.Id.ToString(),
                actionName: maintenanceCategory.TypeName,
                details: $"MaintenanceType details was deleted",
                module: "MaintenanceType");
            await _imediator.Publish(domainEvent);
          

            return  result == -1 ? throw new ExceptionRules("MaintenanceType deletion failed.") : result;
        }
    }
}