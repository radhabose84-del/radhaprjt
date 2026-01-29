using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster
{
    public class CreateActivityMasterCommandHandler  : IRequestHandler<CreateActivityMasterCommand, int>
    {
        private readonly IActivityMasterCommandRepository _activityMasterCommandRepository;
        private readonly IActivityMasterQueryRepository _activityMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        
        public CreateActivityMasterCommandHandler(
            IActivityMasterCommandRepository activityMasterCommandRepository,
            IActivityMasterQueryRepository activityMasterQueryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _activityMasterCommandRepository = activityMasterCommandRepository;
            _activityMasterQueryRepository = activityMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<int> Handle(CreateActivityMasterCommand request, CancellationToken cancellationToken)
            {
                

                // 🔹 Map DTO to domain entity
                var activityMaster = _mapper.Map<MaintenanceManagement.Domain.Entities.ActivityMaster>(request.CreateActivityMasterDto);

                // 🔹 Insert into the database
               var createdActivityMaster = await _activityMasterCommandRepository.CreateAsync(activityMaster);

                // if (createdActivityMaster.Id <= 0)
                // {
                //     return new ApiResponseDTO<int>
                //     {
                //         IsSuccess = false,
                //         Message = "Failed to create Activity",
                //         Data = 0
                //     };
                // }

                // 🔹 Publish domain event for auditing/logging
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: "ActivityMaster",
                    actionName: "Activity Created",
                    details: $"Activity was created.",
                    module: "ActivityMaster"
                );

                await _mediator.Publish(domainEvent, cancellationToken);

                // 🔹 Return success response
                return createdActivityMaster.Id > 0 ? createdActivityMaster.Id : throw new ExceptionRules("ActivityMaster Creation Failed.");
            }


       
    }
}