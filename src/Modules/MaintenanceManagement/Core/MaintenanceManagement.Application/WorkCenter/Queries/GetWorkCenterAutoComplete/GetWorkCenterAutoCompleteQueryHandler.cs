using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterAutoComplete
{
    public class GetWorkCenterAutoCompleteQueryHandler : IRequestHandler<GetWorkCenterAutoCompleteQuery,ApiResponseDTO<List<WorkCenterAutoCompleteDto>>>
    {
        private readonly IWorkCenterQueryRepository _iWorkCenterQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetWorkCenterAutoCompleteQueryHandler(IWorkCenterQueryRepository iWorkCenterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iWorkCenterQueryRepository = iWorkCenterQueryRepository;            
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<WorkCenterAutoCompleteDto>>> Handle(GetWorkCenterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _iWorkCenterQueryRepository.GetWorkCenterGroups(request.SearchPattern);
            var workCenters = _mapper.Map<List<WorkCenterAutoCompleteDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetWorkCenterAutoCompleteQuery",        
                    actionName: workCenters.Count.ToString(),
                    details: $"WorkCenter details was fetched.",
                    module:"WorkCenter"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<WorkCenterAutoCompleteDto>> { IsSuccess = true, Message = "Success", Data = workCenters };
        }
    }
}