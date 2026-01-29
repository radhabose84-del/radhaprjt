using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory
{
    public class DeleteMaintenanceCategoryCommandHandler : IRequestHandler<DeleteMaintenanceCategoryCommand, int>
    {
        private readonly IMaintenanceCategoryCommandRepository _imaintenanceCategoryCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
         public DeleteMaintenanceCategoryCommandHandler(IMaintenanceCategoryCommandRepository imaintenanceCategoryCommandRepository, IMediator imediator, IMapper imapper)
        {
            _imaintenanceCategoryCommandRepository = imaintenanceCategoryCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(DeleteMaintenanceCategoryCommand request, CancellationToken cancellationToken)
        {
             var maintenanceCategory = _imapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(request);
            var result = await _imaintenanceCategoryCommandRepository.DeleteAsync(request.Id,maintenanceCategory);
           

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: maintenanceCategory.Id.ToString(),
                actionName: maintenanceCategory.CategoryName,
                details: $"MaintenanceCategory details was deleted",
                module: "MaintenanceCategory");
            await _imediator.Publish(domainEvent);
          

            return  result == -1 ? throw new ExceptionRules("MaintenanceCategory not found.") : result;
                
        }
    }
}