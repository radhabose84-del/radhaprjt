using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster
{
    public class DeleteActivityCheckListMasterCommandHandler : IRequestHandler<DeleteActivityCheckListMasterCommand, bool>
    {
        private readonly IActivityCheckListMasterCommandRepository _activityCheckListMasterCommandRepository;
         private readonly IActivityCheckListMasterQueryRepository _activityChecklistqueryRepo;
         private readonly IMediator _mediator; 
        private readonly IMapper _imapper;


        public DeleteActivityCheckListMasterCommandHandler(IActivityCheckListMasterCommandRepository activityCheckListMasterCommandRepository, IActivityCheckListMasterQueryRepository activityChecklistqueryRepo, IMediator imediator, IMapper imapper)
        {
            _activityCheckListMasterCommandRepository = activityCheckListMasterCommandRepository;
            _activityChecklistqueryRepo = activityChecklistqueryRepo;
            _mediator = imediator;
            _imapper = imapper;
        }
        
           public async Task<bool> Handle(DeleteActivityCheckListMasterCommand request, CancellationToken cancellationToken)
        {
          

            var activityCheckListMaster = _imapper.Map<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>(request);
             var result = await _activityCheckListMasterCommandRepository.DeleteAsync(request.Id,activityCheckListMaster);


            {
                var linked = await _activityChecklistqueryRepo.IsActivityCheckListMasterLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: activityCheckListMaster.ActivityCheckList,
                actionName:  "ActivityChecklist",   
                details: $"ActivityChecklist details was deleted",
                module: "ActivityChecklist");
            await _mediator.Publish(domainEvent);
          

            return  result== true ? result : throw new ExceptionRules("ActivityChecklist deletion Failed.");
                
        }
        
    }
}