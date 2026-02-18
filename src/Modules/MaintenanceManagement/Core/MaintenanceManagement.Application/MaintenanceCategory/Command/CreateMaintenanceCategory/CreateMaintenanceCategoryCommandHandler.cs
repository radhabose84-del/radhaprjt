#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory
{
    public class CreateMaintenanceCategoryCommandHandler : IRequestHandler<CreateMaintenanceCategoryCommand, int>
    {
        private readonly IMaintenanceCategoryCommandRepository _imaintenanceCategoryCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

          public CreateMaintenanceCategoryCommandHandler(IMaintenanceCategoryCommandRepository imaintenanceCategoryCommandRepository, IMediator imediator, IMapper imapper)
        {
            _imaintenanceCategoryCommandRepository = imaintenanceCategoryCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreateMaintenanceCategoryCommand request, CancellationToken cancellationToken)
        {
              var maintenanceCategory = _imapper.Map<MaintenanceManagement.Domain.Entities.MaintenanceCategory>(request);
            
            var result = await _imaintenanceCategoryCommandRepository.CreateAsync(maintenanceCategory);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: maintenanceCategory.Id.ToString(),
                actionName: maintenanceCategory.CategoryName,
                details: $"MaintenanceCategory details was created",
                module: "MaintenanceCategory");
            await _imediator.Publish(domainEvent, cancellationToken);
          
                    
            return  result > 0 ? result : throw new ExceptionRules("MaintenanceCategory Creation Failed.");
                      
                
           
        }
    }
}