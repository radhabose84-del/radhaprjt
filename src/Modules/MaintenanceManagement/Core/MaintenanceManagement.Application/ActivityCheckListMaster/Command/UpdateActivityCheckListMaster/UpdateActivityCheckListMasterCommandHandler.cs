using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster
{
    public class UpdateActivityCheckListMasterCommandHandler :  IRequestHandler<UpdateActivityCheckListMasterCommand, bool>
    {
         private readonly IActivityCheckListMasterCommandRepository _activityChecklistRepo;
        private readonly IActivityCheckListMasterQueryRepository _activityChecklistqueryRepo;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

         public UpdateActivityCheckListMasterCommandHandler( IActivityCheckListMasterCommandRepository activityChecklistRepo, IActivityCheckListMasterQueryRepository activityChecklistqueryRepo, IMediator mediator,IMapper mapper)
        {
            _activityChecklistRepo = activityChecklistRepo;
            _activityChecklistqueryRepo = activityChecklistqueryRepo;
            _mediator = mediator;
            _mapper = mapper;
        }
         public async Task<bool> Handle(UpdateActivityCheckListMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>(request);

            var result = await _activityChecklistRepo.UpdateAsync(request.Id, entity);

            if (request.IsActive == 0)
            {
                var linked = await _activityChecklistqueryRepo.IsActivityCheckListMasterLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

            //    if (!result)
            //     {
            //         return new ApiResponseDTO<int>
            //         {
            //             IsSuccess = false,
            //             Message = "Activity Checklist not found."
            //         };
            //     }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode:  request.Id.ToString() ,
                actionName: entity.ActivityCheckList ?? "NULL",
                details: $"Activity Checklist was updated",
                module: "ActivityCheckListMaster");

            await _mediator.Publish(domainEvent, cancellationToken);

            return result == true ? result : throw new ExceptionRules("ActivityChecklist update Failed.");
          }


    }
}