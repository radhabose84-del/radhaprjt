using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Domain.Events;
using MassTransit;
using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster
{
    public class CreateActivityCheckListMasterCommandHandler : IRequestHandler<CreateActivityCheckListMasterCommand, int>
    {

         private readonly IActivityCheckListMasterCommandRepository _activityCheckListMasterCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateActivityCheckListMasterCommandHandler(IActivityCheckListMasterCommandRepository activityCheckListMasterCommandRepository, IMediator imediator, IMapper imapper)
        {
            _activityCheckListMasterCommandRepository = activityCheckListMasterCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreateActivityCheckListMasterCommand request, CancellationToken cancellationToken)
        {
            var activityCheckListMaster = _imapper.Map<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>(request);

            var result = await _activityCheckListMasterCommandRepository.CreateAsync(activityCheckListMaster);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: activityCheckListMaster.ActivityId.ToString(),
                actionName: "ActivityChecklist",
                details: $"MachineMaster details was created",
                module: "MachineMaster");
            await _imediator.Publish(domainEvent, cancellationToken);

            // var activityCheckListMasterDto = _imapper.Map<Core.Domain.Entities.ActivityCheckListMaster>(activityCheckListMaster);
            if (result != null)
            {

                return  result.ActivityId > 0 ? result.ActivityId : throw new ExceptionRules("Failed to create DepreciationGroup.");
                

            }
           
             throw new ExceptionRules("ActivityChecklist Creation Failed.");
        }
    }
}