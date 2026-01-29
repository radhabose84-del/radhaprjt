using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetActivityMasterAutoComplete;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete
{
    public class GetActivityMasterAutoCompleteQueryHandler : IRequestHandler<GetActivityMasterAutoCompleteQuery,List<GetActivityMasterAutoCompleteDto>>
    {

        private readonly IActivityMasterQueryRepository _activityMasterQueryRepository  ;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;


         public GetActivityMasterAutoCompleteQueryHandler(IActivityMasterQueryRepository activityMasterQueryRepository  , IMapper mapper, IMediator mediator)
         {
           
           _activityMasterQueryRepository = activityMasterQueryRepository ;
            _mapper =mapper;
            _mediator = mediator;
         }

           public  async Task<List<GetActivityMasterAutoCompleteDto>> Handle(GetActivityMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var machineGroup  = await _activityMasterQueryRepository.GetActivityMasterAutoComplete(request.SearchPattern , request.MachineCode);

            //         if (machineGroup == null || !machineGroup.Any())
            // {
            //     return new ApiResponseDTO<List<GetActivityMasterAutoCompleteDto>>
            //     {
            //         IsSuccess = false,
            //         Message = $"No Activity Master found matching '{request.SearchPattern}'.",
            //         Data = new List<GetActivityMasterAutoCompleteDto>()
            //     };
            // }

            var division = _mapper.Map<List<GetActivityMasterAutoCompleteDto>>(machineGroup);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "", 
                    details: $"Activity Master details was fetched.",
                    module:"Activity Master"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return division;
        }

        
    }
}