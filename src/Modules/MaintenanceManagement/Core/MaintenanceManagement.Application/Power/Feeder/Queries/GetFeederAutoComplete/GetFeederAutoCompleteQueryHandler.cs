#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete
{
    public class GetFeederAutoCompleteQueryHandler : IRequestHandler<GetFeederAutoCompleteQuery, List<GetFeederAutoCompleteDto>>
    {
        private readonly IFeederQueryRepository _feederQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFeederAutoCompleteQueryHandler(IFeederQueryRepository feederQueryRepository, IMapper mapper, IMediator mediator)
        {
            _feederQueryRepository = feederQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        
           public  async Task<List<GetFeederAutoCompleteDto>> Handle(GetFeederAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var machineGroup  = await _feederQueryRepository.GetFeederAutoComplete(request.SearchPattern);


            var division = _mapper.Map<List<GetFeederAutoCompleteDto>>(machineGroup);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "", 
                    details: $"FeederGroup details was fetched.",
                    module:"FeederGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  division; 
        }



        
    }
}