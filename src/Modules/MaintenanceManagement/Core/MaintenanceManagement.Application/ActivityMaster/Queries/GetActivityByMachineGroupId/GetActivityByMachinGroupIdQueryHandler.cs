using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityByMachinGroupId
{
    public class GetActivityByMachinGroupIdQueryHandler : IRequestHandler<GetActivityByMachinGroupIdQuery, List<GetActivityByMachineGroupDto>>
    {

        private readonly IActivityMasterQueryRepository _activityMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetActivityByMachinGroupIdQueryHandler(IActivityMasterQueryRepository activityMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _activityMasterQueryRepository = activityMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        
        public async Task<List<GetActivityByMachineGroupDto>> Handle(GetActivityByMachinGroupIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _activityMasterQueryRepository.GetActivityByMachinGroupId(request.MachineGroupId);

            // if (result == null || !result.Any()) // Check if the list is empty
            // {
            //     return new ApiResponseDTO<List<GetActivityByMachineGroupDto>>
            //     {
            //         IsSuccess = false,
            //         Message = $"Activity Name  for MachineGroup with Id {request.MachineGroupId} not found.",
            //         Data = null
            //     };
            // }

            var ActivityList = _mapper.Map<List<GetActivityByMachineGroupDto>>(result); // Map the list

            

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "",
                actionName: "",
                details: $"Activity Name  for MachineGroupById {request.MachineGroupId} were fetched.",
                module: "Activity Name"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return ActivityList;
        }


    }
}