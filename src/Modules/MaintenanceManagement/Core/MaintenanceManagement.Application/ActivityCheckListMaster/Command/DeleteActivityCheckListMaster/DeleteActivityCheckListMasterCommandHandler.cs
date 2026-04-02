#nullable disable
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

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